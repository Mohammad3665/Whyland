using System.Buffers;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Application.Common.Options;
using Whyland.Domain.Common.Errors;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Local disk storage for episode attachment documents (s7.8, s8.3.1). Files are written into the
/// protected media root under names the server generates; the browser reaches them only over the
/// permission-checked download endpoint, never over static serving.
/// <para>
/// Where the format has a recognizable magic number (PDF, archives) it is verified against the
/// declared file kind; everything else is bounded by the extension allow-list and the size limit.
/// Internal failures are translation points, not user-facing text: this adapter turns them into
/// the application's error codes so the boundary resolves the wording.
/// </para>
/// </summary>
public sealed class LocalAttachmentStorageService(
    IOptions<EpisodeAttachmentOptions> options,
    IHostEnvironment environment,
    ILogger<LocalAttachmentStorageService> logger) : IAttachmentStorageService
{
    private const int BufferSize = 81920;

    /// <summary>Directory segment the attachment files live in, inside the protected root.</summary>
    private const string AttachmentDirectory = "files";

    private readonly EpisodeAttachmentOptions _options = options.Value;
    private readonly SemaphoreSlim _processingGate = new(1, 1);

    /// <inheritdoc />
    public async Task<Result<StoredFile>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        await _processingGate.WaitAsync(ct);

        try
        {
            return await SaveCoreAsync(content, fileName, contentType, contentLength, ct);
        }
        catch (ImageStorageFileTooLargeException)
        {
            return Result<StoredFile>.Failure(SystemErrors.AttachmentTooLarge());
        }
        catch (InvalidDataException ex)
        {
            // The reason stays in the log; the caller only receives the stable code.
            logger.LogInformation(ex, "Rejected an invalid attachment upload ({DeclaredLength} declared bytes).", contentLength);
            return Result<StoredFile>.Failure(SystemErrors.AttachmentNotAllowed());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            logger.LogError(ex, "Storing an episode attachment failed.");
            return Result<StoredFile>.Failure(SystemErrors.AttachmentStorageFailed());
        }
        finally
        {
            _processingGate.Release();
        }
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        if (!ProtectedStorageKeys.IsValid(storagePath, AttachmentDirectory))
            return Task.FromResult(Result.Failure(SystemErrors.AttachmentStorageFailed()));

        try
        {
            File.Delete(Path.Combine(ResolveRoot(), Normalize(storagePath)));

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Deleting attachment {StoragePath} failed.", storagePath);
            return Task.FromResult(Result.Failure(SystemErrors.AttachmentStorageFailed()));
        }
    }

    /// <inheritdoc />
    public Task<Result<VideoFileStream>> OpenAsync(string storagePath, CancellationToken ct = default)
    {
        if (!ProtectedStorageKeys.IsValid(storagePath, AttachmentDirectory))
            return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.AttachmentStorageFailed()));

        try
        {
            var path = Path.Combine(ResolveRoot(), Normalize(storagePath));

            var info = new FileInfo(path);
            if (!info.Exists || info.Length <= 0)
            {
                logger.LogWarning("Opening attachment {StoragePath} failed: the file is missing or empty.", storagePath);
                return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.AttachmentStorageFailed()));
            }

            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return Task.FromResult(Result<VideoFileStream>.Success(new VideoFileStream(stream, info.Length)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.LogError(ex, "Opening attachment {StoragePath} for download failed.", storagePath);
            return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.AttachmentStorageFailed()));
        }
    }

    /// <inheritdoc />
    public string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var name = Path.GetFileName(fileName.Trim());

        // Strip control characters Windows and browsers may smuggle into the advertised name.
        var safeChars = name.Where(c => !char.IsControl(c));
        var safe = new string(safeChars.ToArray()).Trim();

        return safe.Length > 256 ? safe[..256] : safe;
    }

    private async Task<Result<StoredFile>> SaveCoreAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct)
    {
        var attachmentDirectory = Path.Combine(ResolveRoot(), AttachmentDirectory);
        Directory.CreateDirectory(attachmentDirectory);

        if (contentLength <= 0)
            throw new InvalidDataException("The uploaded file size is invalid.");

        if (contentLength > _options.MaxAttachmentSizeBytes)
            throw new ImageStorageFileTooLargeException(_options.MaxAttachmentSizeBytes);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) ||
            !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("The file extension is not on the configured allow-list.");

        var mediaType = ResolveContentType(extension, contentType);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var tempPath = Path.Combine(attachmentDirectory, $".{storedFileName}.upload.tmp");
        var finalPath = Path.Combine(attachmentDirectory, storedFileName);

        try
        {
            var storedLength = await CopyToBoundedFileAsync(content, tempPath, contentLength, ct);

            await ValidateSignatureAsync(tempPath, extension, ct);

            File.Move(tempPath, finalPath);

            return Result<StoredFile>.Success(
                new StoredFile($"{AttachmentDirectory}/{storedFileName}", storedLength, mediaType));
        }
        finally
        {
            TryDelete(tempPath);
        }
    }

    /// <summary>
    /// Prefers the transport's declared media type when one was sent and falls back to the
    /// canonical type of the extension; an empty or opaque declaration is never recorded as-is.
    /// </summary>
    private static string ResolveContentType(string extension, string? contentType)
    {
        var mediaType = string.IsNullOrWhiteSpace(contentType)
            ? null
            : contentType.Split(';', 2)[0].Trim();

        if (mediaType is null || mediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            mediaType = null;

        if (mediaType is null)
            return CanonicalMediaTypes.TryGetValue(extension, out var canonical) ? canonical : "application/octet-stream";

        return mediaType;
    }

    /// <summary>The canonical MIME type per accepted extension; every accepted extension has one.</summary>
    private static readonly Dictionary<string, string> CanonicalMediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".zip"] = "application/zip",
        [".rar"] = "application/vnd.rar",
        [".7z"] = "application/x-7z-compressed",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".txt"] = "text/plain",
        [".mp3"] = "audio/mpeg",
    };

    /// <summary>
    /// Reads just enough of the file to confirm the signature of the formats the platform accepts
    /// and can cheaply recognize; a mismatch makes the whole upload unacceptable.
    /// </summary>
    private static async Task ValidateSignatureAsync(string path, string extension, CancellationToken ct)
    {
        byte[] expectedSignatures = extension switch
        {
            ".pdf" => "%PDF"u8.ToArray(),
            ".zip" or ".docx" or ".xlsx" or ".pptx" => [0x50, 0x4B, 0x03, 0x04],
            ".rar" => "Rar!"u8.ToArray(),
            _ => [],
        };

        if (expectedSignatures.Length == 0)
            return;

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var header = new byte[expectedSignatures.Length];
        var total = 0;

        while (total < header.Length)
        {
            var count = await stream.ReadAsync(header.AsMemory(total), ct);
            if (count == 0)
                throw new InvalidDataException("The uploaded file is shorter than its signature requires.");

            total += count;
        }

        if (!header.AsSpan().SequenceEqual(expectedSignatures))
            throw new InvalidDataException("The file signature does not match the declared file kind.");
    }

    /// <summary>
    /// Copies the upload to the target file, refusing anything beyond the configured ceiling or
    /// inconsistent with the declared length, and returns the number of bytes actually read.
    /// </summary>
    private async Task<long> CopyToBoundedFileAsync(
        Stream source,
        string destinationPath,
        long expectedLength,
        CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long totalBytes = 0;

        try
        {
            // The writer is closed before the signature is read, so validation never races an open
            // FileShare.None handle and always sees the complete upload.
            await using (var destination = new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous))
            {
                while (true)
                {
                    var bytesRead = await source.ReadAsync(buffer.AsMemory(0, BufferSize), ct);
                    if (bytesRead == 0)
                        break;

                    totalBytes += bytesRead;

                    if (totalBytes > _options.MaxAttachmentSizeBytes)
                        throw new ImageStorageFileTooLargeException(_options.MaxAttachmentSizeBytes);

                    if (totalBytes > expectedLength)
                        throw new InvalidDataException("The uploaded file is longer than declared.");

                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                }

                await destination.FlushAsync(ct);
            }

            if (totalBytes == 0 || totalBytes != expectedLength)
                throw new InvalidDataException("The uploaded file is empty or its length does not match the request.");

            return totalBytes;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private string ResolveRoot() => ProtectedStorageKeys.ResolveRootPath(_options.RootPath, environment);

    private static string Normalize(string key) => key.Trim().Replace('\\', '/');

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
}
