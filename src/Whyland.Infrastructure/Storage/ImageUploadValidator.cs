using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Validates uploaded images before anything is published: the extension, the declared content type,
/// the binary signature and finally a real decode (s8.3.1). The decoded image is returned so the
/// caller can resize and re-encode it instead of trusting the uploaded bytes.
/// </summary>
public sealed class ImageUploadValidator(IOptions<LocalImageStorageOptions> options)
{
    private readonly LocalImageStorageOptions _options = options.Value;

    private static readonly Dictionary<string, ImageFileFormat> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ImageFileFormat.Jpeg,
        [".jpeg"] = ImageFileFormat.Jpeg,
        [".png"] = ImageFileFormat.Png,
        [".webp"] = ImageFileFormat.WebP
    };

    private static readonly Dictionary<ImageFileFormat, string[]> KnownContentTypes = new()
    {
        [ImageFileFormat.Jpeg] = ["image/jpeg", "application/octet-stream"],
        [ImageFileFormat.Png] = ["image/png", "application/octet-stream"],
        [ImageFileFormat.WebP] = ["image/webp", "application/octet-stream"]
    };

    /// <summary>
    /// Checks the extension, declared content type, binary signature and image structure, and decodes
    /// the image. The original dimensions are preserved for the resize decision.
    /// </summary>
    /// <exception cref="InvalidDataException">The upload is not one of the supported image formats.</exception>
    internal async Task<ValidatedImage> ValidateAndLoadAsync(
        string filePath,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidDataException("The uploaded file name is required.");

        var extension = Path.GetExtension(fileName);

        if (!SupportedExtensions.TryGetValue(extension, out var extensionFormat))
            throw new InvalidDataException("Only JPG, JPEG, PNG and WebP images are supported.");

        ValidateContentType(contentType, extensionFormat);

        var detectedFormat = await DetectFormatAsync(filePath, cancellationToken);

        if (detectedFormat != extensionFormat)
            throw new InvalidDataException("The file extension does not match the actual image format.");

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        ImageInfo? imageInfo;

        try
        {
            imageInfo = await Image.IdentifyAsync(stream, cancellationToken);
        }
        catch (Exception ex) when (IsInvalidImageException(ex))
        {
            throw new InvalidDataException("The uploaded file is not a valid image.", ex);
        }

        if (imageInfo is null)
            throw new InvalidDataException("The uploaded file is not a valid image.");

        // Preserve the real file dimensions before decoding and resizing.
        var sourceWidth = imageInfo.Width;
        var sourceHeight = imageInfo.Height;

        ValidateDimensions(sourceWidth, sourceHeight);

        stream.Position = 0;

        var decoderOptions = new DecoderOptions
        {
            MaxFrames = 1,
            SkipMetadata = false,
            TargetSize = new Size(_options.MaxOriginalDimension, _options.MaxOriginalDimension),
            Sampler = KnownResamplers.Lanczos3,
            SegmentIntegrityHandling = SegmentIntegrityHandling.Strict
        };

        Image image;

        try
        {
            image = await Image.LoadAsync(decoderOptions, stream, cancellationToken);
        }
        catch (Exception ex) when (IsInvalidImageException(ex))
        {
            throw new InvalidDataException("The uploaded file could not be decoded as a valid image.", ex);
        }

        try
        {
            var decodedFormat = image.Metadata.DecodedImageFormat?.Name;

            if (decodedFormat is not null &&
                !string.Equals(decodedFormat, GetFormatName(detectedFormat), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The decoded image format does not match the detected format.");
            }

            image.Mutate(x => x.AutoOrient());

            ValidateDimensions(image.Width, image.Height);

            return new ValidatedImage(image, detectedFormat, sourceWidth, sourceHeight);
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    /// <summary>Matches the declared content type to the format the extension promises.</summary>
    private static void ValidateContentType(string? contentType, ImageFileFormat format)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return;

        var mediaType = contentType.Split(';', 2)[0].Trim();

        if (!KnownContentTypes[format].Contains(mediaType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("The content type does not match the file extension.");
    }

    /// <summary>Detects the image format from the file's binary signature.</summary>
    private static async Task<ImageFileFormat> DetectFormatAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var header = new byte[12];

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var bytesRead = 0;

        while (bytesRead < header.Length)
        {
            var count = await stream.ReadAsync(header.AsMemory(bytesRead), cancellationToken);

            if (count == 0)
                break;

            bytesRead += count;
        }

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return ImageFileFormat.Jpeg;
        }

        if (bytesRead >= 8 &&
            header.AsSpan(0, 8).SequenceEqual(
                new byte[]
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                }))
        {
            return ImageFileFormat.Png;
        }

        if (bytesRead >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return ImageFileFormat.WebP;

        throw new InvalidDataException("The file signature is not a supported image format.");
    }

    /// <summary>Enforces the configured dimension and pixel-count limits.</summary>
    private void ValidateDimensions(int width, int height)
    {
        if (width <= 0 ||
            height <= 0 ||
            width > _options.MaxImageWidth ||
            height > _options.MaxImageHeight ||
            (long)width * height > _options.MaxImagePixels)
        {
            throw new InvalidDataException("The image dimensions exceed the allowed limits.");
        }
    }

    private static string GetFormatName(ImageFileFormat format) => format switch
    {
        ImageFileFormat.Jpeg => "JPEG",
        ImageFileFormat.Png => "PNG",
        ImageFileFormat.WebP => "WEBP",
        _ => throw new InvalidOperationException("Unsupported image format.")
    };

    private static bool IsInvalidImageException(Exception ex) =>
        ex is UnknownImageFormatException or
            InvalidImageContentException or
            ArgumentException or
            FormatException or
            OverflowException;
}

/// <summary>The image formats proven by the file signature and accepted by the validator.</summary>
internal enum ImageFileFormat
{
    Jpeg,
    Png,
    WebP
}
