using System.Buffers;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Domain.Common.Errors;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Stores images on local disk: validation, optimization, optional WebP conversion and thumbnail
/// generation (s8.3.1, s8.3.2). Files are written under the host's content root and named by the
/// server, so a caller can neither choose a path nor overwrite another image.
/// <para>
/// Internal failures are translation points, not user-facing text: this adapter turns them into the
/// application's error codes so the boundary resolves the wording.
/// </para>
/// </summary>
public sealed class LocalImageStorageService : IImageStorageService, IDisposable
{
    private const int BufferSize = 81920;
    private const int PngMaxColors = 256;
    private const float PngDitherScale = 0.65f;

    private readonly LocalImageStorageOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ImageUploadValidator _imageUploadValidator;
    private readonly ILogger<LocalImageStorageService> _logger;
    private readonly SemaphoreSlim _processingGate;

    public LocalImageStorageService(
        IOptions<LocalImageStorageOptions> options,
        IHostEnvironment environment,
        ImageUploadValidator imageUploadValidator,
        ILogger<LocalImageStorageService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _imageUploadValidator = imageUploadValidator;
        _logger = logger;
        _processingGate = new SemaphoreSlim(_options.MaxConcurrentOperations, _options.MaxConcurrentOperations);
    }

    /// <inheritdoc />
    public async Task<Result<StoredImage>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        string areaKey,
        ImageStorageSaveOptions options,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            return await SaveCoreAsync(content, fileName, contentType, contentLength, areaKey, options, ct);
        }
        catch (ImageStorageFileTooLargeException ex)
        {
            _logger.LogInformation(
                "Rejected an image upload for {Area}: {SizeBytes} bytes exceeds the {MaxBytes} byte limit.",
                areaKey,
                contentLength,
                ex.MaxBytes);

            return SystemErrors.ImageTooLarge();
        }
        catch (InvalidDataException ex)
        {
            // The reason stays in the log; the caller only receives the stable code.
            _logger.LogInformation(ex, "Rejected an invalid image upload for {Area}.", areaKey);

            return SystemErrors.InvalidImage();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            _logger.LogError(ex, "Storing an image for {Area} failed.", areaKey);

            return SystemErrors.ImageStorageFailed();
        }
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string areaKey, string fileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            ValidateStoredFileName(fileName);

            var paths = GetPaths(areaKey);
            File.Delete(Path.Combine(paths.OriginalDirectoryPath, fileName));
            File.Delete(Path.Combine(paths.ThumbnailDirectoryPath, fileName));

            return Task.FromResult(Result.Success());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Deleting image {FileName} from {Area} failed.", fileName, areaKey);

            return Task.FromResult(Result.Failure(SystemErrors.ImageStorageFailed()));
        }
    }

    /// <summary>
    /// Validates the upload, writes the processed image and its optional thumbnail, and moves both
    /// into place only after every step succeeded.
    /// </summary>
    private async Task<Result<StoredImage>> SaveCoreAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        string areaKey,
        ImageStorageSaveOptions options,
        CancellationToken cancellationToken)
    {
        // Check the declared size before touching the disk, so a huge file is refused early.
        if (contentLength <= 0)
            throw new InvalidDataException("The uploaded file size is invalid.");

        if (contentLength > _options.MaxFileSizeBytes)
            throw new ImageStorageFileTooLargeException(_options.MaxFileSizeBytes);

        var paths = GetPaths(areaKey);
        await _processingGate.WaitAsync(cancellationToken);

        var operationId = Guid.NewGuid().ToString("N");
        string? inputTempPath = null;
        string? tempOriginalPath = null;
        string? tempThumbnailPath = null;
        string? finalOriginalPath = null;
        string? finalThumbnailPath = null;

        try
        {
            Directory.CreateDirectory(paths.OriginalDirectoryPath);

            if (options.GenerateThumbnail)
                Directory.CreateDirectory(paths.ThumbnailDirectoryPath);

            inputTempPath = Path.Combine(paths.OriginalDirectoryPath, $".{operationId}.upload.tmp");
            tempOriginalPath = Path.Combine(paths.OriginalDirectoryPath, $".{operationId}.original.tmp");

            await CopyToBoundedFileAsync(content, inputTempPath, contentLength, cancellationToken);

            using var validatedImage = await _imageUploadValidator.ValidateAndLoadAsync(
                inputTempPath,
                fileName,
                contentType,
                cancellationToken);

            var image = validatedImage.Image;
            var sourceRequiresResize =
                Math.Max(validatedImage.SourceWidth, validatedImage.SourceHeight) > _options.MaxOriginalDimension;

            ResizeDown(image, _options.MaxOriginalDimension);

            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var outputFormat = options.ConvertToWebP ? ImageFileFormat.WebP : validatedImage.Format;
            var storedFileName = $"{operationId}{GetExtension(outputFormat)}";

            finalOriginalPath = Path.Combine(paths.OriginalDirectoryPath, storedFileName);

            // Without processing or resizing, and already under the output ceiling, the uploaded file
            // is kept as-is instead of paying for an unnecessary re-encode.
            if (options is { OptimizeImage: false, ConvertToWebP: false } &&
                !sourceRequiresResize &&
                contentLength < _options.MaxOutputImageSizeBytes)
            {
                await CopyFileAsync(inputTempPath, tempOriginalPath, cancellationToken);
            }
            else
            {
                // The ceiling applies to every format; quality and then dimensions are reduced as needed.
                var outputDimensions = await SaveImageWithinSizeLimitAsync(
                    image,
                    tempOriginalPath,
                    outputFormat,
                    _options.MaxOutputImageSizeBytes,
                    options.OptimizeImage,
                    cancellationToken);

                originalWidth = outputDimensions.Width;
                originalHeight = outputDimensions.Height;
            }

            if (options.GenerateThumbnail)
            {
                tempThumbnailPath = Path.Combine(paths.ThumbnailDirectoryPath, $".{operationId}.thumbnail.tmp");
                finalThumbnailPath = Path.Combine(paths.ThumbnailDirectoryPath, storedFileName);

                // The thumbnail is a clone, so shrinking it never affects the stored original.
                using var thumbnailImage = image.CloneAs<Rgba32>();
                ResizeDown(thumbnailImage, _options.ThumbnailMaxDimension);

                await SaveImageWithinSizeLimitAsync(
                    thumbnailImage,
                    tempThumbnailPath,
                    outputFormat,
                    _options.MaxThumbnailSizeBytes,
                    optimize: true,
                    cancellationToken);
            }

            File.Move(tempOriginalPath, finalOriginalPath);

            if (tempThumbnailPath is not null && finalThumbnailPath is not null)
                File.Move(tempThumbnailPath, finalThumbnailPath);

            return new StoredImage(
                storedFileName,
                BuildRelativePath(paths.AreaDirectoryName, _options.OriginalDirectory, storedFileName),
                options.GenerateThumbnail
                    ? BuildRelativePath(paths.AreaDirectoryName, _options.ThumbnailDirectory, storedFileName)
                    : null,
                originalWidth,
                originalHeight,
                new FileInfo(finalOriginalPath).Length);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
        {
            Cleanup(tempOriginalPath, tempThumbnailPath, finalOriginalPath, finalThumbnailPath);
            throw new InvalidDataException("The uploaded file could not be decoded as a valid image.", ex);
        }
        catch
        {
            Cleanup(tempOriginalPath, tempThumbnailPath, finalOriginalPath, finalThumbnailPath);
            throw;
        }
        finally
        {
            TryDelete(inputTempPath);
            _processingGate.Release();
        }
    }

    /// <summary>
    /// Copies the upload to a bounded temporary file. The real byte count is enforced, not just the
    /// declared Content-Length, so a lying request cannot fill the disk.
    /// </summary>
    private async Task CopyToBoundedFileAsync(
        Stream source,
        string destinationPath,
        long expectedLength,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long totalBytes = 0;

        try
        {
            await using var destination = new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            while (true)
            {
                var bytesRead = await source.ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken);
                if (bytesRead == 0)
                    break;

                totalBytes += bytesRead;

                if (totalBytes > _options.MaxFileSizeBytes)
                    throw new ImageStorageFileTooLargeException(_options.MaxFileSizeBytes);

                if (totalBytes > expectedLength)
                    throw new InvalidDataException("The uploaded file length is invalid.");

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            if (totalBytes == 0 || totalBytes != expectedLength)
                throw new InvalidDataException("The uploaded file is empty or its length is invalid.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var source = new FileStream(
            sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using var destination = new FileStream(
            destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await source.CopyToAsync(destination, BufferSize, cancellationToken);
    }

    /// <summary>
    /// Encodes the image under the given size ceiling. Quality (or the PNG palette) is reduced first;
    /// if that is not enough, dimensions are stepped down while keeping the aspect ratio.
    /// </summary>
    private async Task<(int Width, int Height)> SaveImageWithinSizeLimitAsync(
        Image image,
        string destinationPath,
        ImageFileFormat format,
        long maxSizeBytes,
        bool optimize,
        CancellationToken cancellationToken)
    {
        if (maxSizeBytes <= 0)
            throw new InvalidOperationException("The configured output image size limit must be greater than zero.");

        const int minimumDimension = 64;
        var candidatePaths = new List<string>();
        var candidateIndex = 0;

        async Task<(string Path, long Size)> EncodeCandidateAsync(Image source, int quality, int maxColors, bool usePalette)
        {
            var candidatePath = $"{destinationPath}.{candidateIndex++}.candidate";
            candidatePaths.Add(candidatePath);

            IImageEncoder encoder = format switch
            {
                ImageFileFormat.Jpeg => new JpegEncoder
                {
                    Quality = quality,
                    SkipMetadata = true
                },
                ImageFileFormat.Png when usePalette => new PngEncoder
                {
                    ColorType = PngColorType.Palette,
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    FilterMethod = PngFilterMethod.Adaptive,
                    Quantizer = new WuQuantizer(new QuantizerOptions
                    {
                        MaxColors = maxColors,
                        Dither = KnownDitherings.FloydSteinberg,
                        DitherScale = PngDitherScale
                    }),
                    SkipMetadata = true
                },
                ImageFileFormat.Png => new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    SkipMetadata = true
                },
                ImageFileFormat.WebP => new WebpEncoder
                {
                    FileFormat = WebpFileFormatType.Lossy,
                    Quality = quality,
                    SkipMetadata = true
                },
                _ => throw new InvalidOperationException("Unsupported image format.")
            };

            await source.SaveAsync(candidatePath, encoder, cancellationToken);
            return (candidatePath, new FileInfo(candidatePath).Length);
        }

        async Task<string?> TryEncodeAtCurrentDimensionsAsync(Image source)
        {
            if (format == ImageFileFormat.Png)
            {
                // With optimization explicitly off, a lossless PNG is tried before any quantization.
                if (!optimize)
                {
                    var losslessCandidate = await EncodeCandidateAsync(source, 100, PngMaxColors, usePalette: false);
                    if (losslessCandidate.Size < maxSizeBytes)
                        return losslessCandidate.Path;
                }

                // Reduce the palette from 256 colors down, to keep the visual loss as low as possible.
                foreach (var maxColors in new[] { 256, 128, 64, 32, 16 })
                {
                    var candidate = await EncodeCandidateAsync(source, 100, maxColors, usePalette: true);
                    if (candidate.Size < maxSizeBytes)
                        return candidate.Path;
                }

                return null;
            }

            var startingQuality = optimize
                ? (format == ImageFileFormat.Jpeg ? _options.JpegQuality : _options.WebPQuality)
                : 95;

            var qualities = new List<int> { Math.Clamp(startingQuality, 30, 100) };
            for (var quality = qualities[0] - 10; quality > 30; quality -= 10)
                qualities.Add(quality);
            if (qualities[^1] != 30)
                qualities.Add(30);

            foreach (var quality in qualities)
            {
                var candidate = await EncodeCandidateAsync(source, quality, PngMaxColors, usePalette: false);
                if (candidate.Size < maxSizeBytes)
                    return candidate.Path;
            }

            return null;
        }

        try
        {
            // First keep the current dimensions and only tune the encoder.
            var selectedPath = await TryEncodeAtCurrentDimensionsAsync(image);
            if (selectedPath is not null)
            {
                File.Move(selectedPath, destinationPath);
                return (image.Width, image.Height);
            }

            // Quality/colors were not enough, so step the dimensions down.
            var currentMaxDimension = Math.Max(image.Width, image.Height);
            while (currentMaxDimension > minimumDimension)
            {
                var nextMaxDimension = Math.Max(minimumDimension, (int)(currentMaxDimension * 0.85));
                if (nextMaxDimension >= currentMaxDimension)
                    break;

                using var resizedImage = image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(nextMaxDimension, nextMaxDimension),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));

                selectedPath = await TryEncodeAtCurrentDimensionsAsync(resizedImage);
                if (selectedPath is not null)
                {
                    File.Move(selectedPath, destinationPath);
                    return (resizedImage.Width, resizedImage.Height);
                }

                currentMaxDimension = nextMaxDimension;
            }

            throw new InvalidOperationException(
                $"The image could not be reduced below the configured size limit of {maxSizeBytes} bytes.");
        }
        finally
        {
            foreach (var path in candidatePaths)
                TryDelete(path);
        }
    }

    private static void ResizeDown(Image image, int maxDimension)
    {
        if (Math.Max(image.Width, image.Height) <= maxDimension)
            return;

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(maxDimension, maxDimension),
            Mode = ResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));
    }

    private static string GetExtension(ImageFileFormat format) => format switch
    {
        ImageFileFormat.Jpeg => ".jpg",
        ImageFileFormat.Png => ".png",
        ImageFileFormat.WebP => ".webp",
        _ => throw new InvalidOperationException("Unsupported image format.")
    };

    private StoragePaths GetPaths(string areaKey)
    {
        var areaDirectoryName = ResolveAreaDirectoryName(areaKey);
        var rootPath = Path.IsPathRooted(_options.RootPath)
            ? Path.GetFullPath(_options.RootPath)
            : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));

        return new StoragePaths(
            areaDirectoryName,
            Path.Combine(rootPath, areaDirectoryName, _options.OriginalDirectory),
            Path.Combine(rootPath, areaDirectoryName, _options.ThumbnailDirectory));
    }

    private string ResolveAreaDirectoryName(string areaKey)
    {
        if (string.IsNullOrWhiteSpace(areaKey))
            throw new ArgumentException("An image storage area key is required.", nameof(areaKey));

        foreach (var area in _options.Areas)
        {
            if (!string.Equals(area.Key, areaKey, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!IsSafeDirectoryName(area.Value))
                throw new InvalidOperationException($"The configured directory for '{areaKey}' is invalid.");

            return area.Value;
        }

        throw new InvalidOperationException($"Image storage area '{areaKey}' is not configured.");
    }

    private static bool IsSafeDirectoryName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value is not "." and not ".." &&
        !Path.IsPathRooted(value) &&
        !value.Contains('/') &&
        !value.Contains('\\');

    private static string BuildRelativePath(string area, string directory, string fileName) =>
        $"{area}/{directory}/{fileName}";

    private static void ValidateStoredFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Contains('/') ||
            fileName.Contains('\\') ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) ||
            Path.GetExtension(fileName).ToLowerInvariant() is not (".jpg" or ".png" or ".webp"))
        {
            throw new ArgumentException("The stored image file name is invalid.", nameof(fileName));
        }
    }

    private static void Cleanup(params string?[] paths)
    {
        foreach (var path in paths)
            TryDelete(path);
    }

    private static void TryDelete(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public void Dispose() => _processingGate.Dispose();

    private sealed record StoragePaths(string AreaDirectoryName, string OriginalDirectoryPath, string ThumbnailDirectoryPath);
}
