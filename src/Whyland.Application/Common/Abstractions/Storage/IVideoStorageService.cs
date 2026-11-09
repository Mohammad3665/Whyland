namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Stores course videos on local disk after validating the container (s8.1.1, s8.3.2).
/// <para>
/// Files live in the protected media root whose static serving is blocked; they are only ever read
/// back through <see cref="OpenAsync"/> behind the playback endpoint that checks permission first.
/// Storage keys are generated here, never sent by the caller, so a caller can neither choose a path
/// nor overwrite another video.
/// </para>
/// </summary>
public interface IVideoStorageService
{
    /// <summary>
    /// Validates the upload (extension, declared content type, MP4 container signature and duration)
    /// and stores it under the episode-video area. The original bytes are kept; no re-encoding happens.
    /// </summary>
    /// <param name="content">The uploaded content stream.</param>
    /// <param name="fileName">The original file name, used only to check the extension.</param>
    /// <param name="contentType">The declared content type, when the transport provides one.</param>
    /// <param name="contentLength">The declared length, re-checked against the bytes actually read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored video, or the stable storage error on rejection.</returns>
    Task<Result<StoredVideo>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a previously stored video. A file that is already gone is not an error.
    /// </summary>
    /// <param name="videoKey">The server-generated relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result> DeleteAsync(string videoKey, CancellationToken ct = default);

    /// <summary>
    /// Opens a stored video for streaming. The caller closes the returned stream when done, and is
    /// responsible for having authorized the request before opening it.
    /// </summary>
    /// <param name="videoKey">The server-generated relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result<VideoFileStream>> OpenAsync(string videoKey, CancellationToken ct = default);
}
