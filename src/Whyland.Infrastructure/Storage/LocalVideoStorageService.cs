using System.Buffers;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Application.Common.Options;
using Whyland.Domain.Common.Errors;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Local disk storage for course episode videos (s8.1.1, s8.3.2). Files are written into the
/// protected media root, named by the server and never re-encoded; the browser reaches them only
/// over the permission-checked streaming endpoint, never over static serving.
/// <para>
/// Internal failures are translation points, not user-facing text: this adapter turns them into
/// the application's error codes so the boundary resolves the wording.
/// </para>
/// </summary>
public sealed class LocalVideoStorageService(
    IOptions<EpisodeVideoOptions> options,
    IHostEnvironment environment,
    ILogger<LocalVideoStorageService> logger) : IVideoStorageService, IDisposable
{
    private const int BufferSize = 81920;

    private static readonly string[] AllowedExtensions = [".mp4"];

    private static readonly string[] AllowedMediaTypes =
        ["video/mp4", "video/x-m4v", "application/octet-stream"];

    /// <summary>Directory segment the episode videos live in, inside the protected root.</summary>
    private const string VideoDirectory = "videos";

    private readonly EpisodeVideoOptions _options = options.Value;
    private readonly IHostEnvironment _environment = environment;
    private readonly SemaphoreSlim _processingGate = new(1, 1);

    /// <inheritdoc />
    public async Task<Result<StoredVideo>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // Videos are copied and probed one at a time, so parallel uploads cannot thrash the disk.
        await _processingGate.WaitAsync(ct);

        try
        {
            return await SaveCoreAsync(content, fileName, contentType, contentLength, ct);
        }
        catch (ImageStorageFileTooLargeException)
        {
            return Result<StoredVideo>.Failure(SystemErrors.VideoTooLarge());
        }
        catch (InvalidDataException ex)
        {
            // The reason stays in the log; the caller only receives the stable code.
            logger.LogInformation(ex, "Rejected an invalid video upload ({DeclaredLength} declared bytes).", contentLength);
            return Result<StoredVideo>.Failure(SystemErrors.InvalidVideo());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            logger.LogError(ex, "Storing an episode video failed.");
            return Result<StoredVideo>.Failure(SystemErrors.VideoStorageFailed());
        }
        finally
        {
            _processingGate.Release();
        }
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string videoKey, CancellationToken ct = default)
    {
        if (!ProtectedStorageKeys.IsValid(videoKey, VideoDirectory))
            return Task.FromResult(Result.Failure(SystemErrors.VideoStorageFailed()));

        try
        {
            File.Delete(Path.Combine(ResolveRoot(), Normalize(videoKey)));

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Deleting video {VideoKey} failed.", videoKey);
            return Task.FromResult(Result.Failure(SystemErrors.VideoStorageFailed()));
        }
    }

    /// <inheritdoc />
    public Task<Result<VideoFileStream>> OpenAsync(string videoKey, CancellationToken ct = default)
    {
        if (!ProtectedStorageKeys.IsValid(videoKey, VideoDirectory))
            return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.VideoStorageFailed()));

        try
        {
            var path = Path.Combine(ResolveRoot(), Normalize(videoKey));

            var info = new FileInfo(path);
            if (!info.Exists || info.Length <= 0)
            {
                logger.LogWarning("Opening video {VideoKey} failed: the file is missing or empty.", videoKey);
                return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.VideoStorageFailed()));
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
            logger.LogError(ex, "Opening video {VideoKey} for streaming failed.", videoKey);
            return Task.FromResult(Result<VideoFileStream>.Failure(SystemErrors.VideoStorageFailed()));
        }
    }

    private async Task<Result<StoredVideo>> SaveCoreAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct)
    {
        var videoDirectory = Path.Combine(ResolveRoot(), VideoDirectory);
        Directory.CreateDirectory(videoDirectory);

        // The declared length is checked before any byte reaches the disk, so an oversized upload is
        // refused without a useless copy.
        if (contentLength <= 0)
            throw new InvalidDataException("The uploaded file size is invalid.");

        if (contentLength > _options.MaxFileSizeBytes)
            throw new ImageStorageFileTooLargeException(_options.MaxFileSizeBytes);

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("Only MP4 videos are supported.");

        ValidateDeclaredContentType(contentType);

        var storedFileName = $"{Guid.NewGuid():N}.mp4";
        var tempPath = Path.Combine(videoDirectory, $".{storedFileName}.upload.tmp");
        var finalPath = Path.Combine(videoDirectory, storedFileName);

        try
        {
            // The declared length alone is not trusted; the byte count is re-checked while copying.
            var storedLength = await CopyToBoundedFileAsync(content, tempPath, contentLength, ct);

            await using var probeStream = new FileStream(
                tempPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            if (!await Mp4ContainerProbe.HasFtypAsync(probeStream, ct))
                throw new InvalidDataException("The uploaded file is not an MP4 container.");

            var duration = await Mp4ContainerProbe.ReadDurationAsync(probeStream, ct);

            if (duration is not { } realDuration || realDuration <= TimeSpan.Zero)
                throw new InvalidDataException("The MP4 video header could not provide a usable duration.");

            var maxDuration = TimeSpan.FromSeconds(_options.MaxDurationSeconds);
            if (realDuration > maxDuration)
                throw new InvalidDataException(
                    $"The video is {realDuration.TotalSeconds:F0}s long; the configured maximum is {maxDuration.TotalSeconds:F0}s.");

            var stored = TimeSpan.FromSeconds(
                Math.Round(realDuration.TotalSeconds, MidpointRounding.AwayFromZero));

            File.Move(tempPath, finalPath);

            return Result<StoredVideo>.Success(
                new StoredVideo($"{VideoDirectory}/{storedFileName}", storedLength, stored));
        }
        finally
        {
            TryDelete(tempPath);
        }
    }

    private void ValidateDeclaredContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return;

        var mediaType = contentType.Split(';', 2)[0].Trim();

        if (!AllowedMediaTypes.Contains(mediaType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("The declared content type does not match an MP4 video.");
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
            // The writer is closed before the caller probes the file, so the probe never races an
            // open FileShare.None handle and always sees the complete upload.
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

                    if (totalBytes > _options.MaxFileSizeBytes)
                        throw new ImageStorageFileTooLargeException(_options.MaxFileSizeBytes);

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

    private string ResolveRoot() => ProtectedStorageKeys.ResolveRootPath(_options.RootPath, _environment);

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

    /// <inheritdoc />
    public void Dispose() => _processingGate.Dispose();
}
