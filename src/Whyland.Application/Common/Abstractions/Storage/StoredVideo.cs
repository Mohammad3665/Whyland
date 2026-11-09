namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Metadata of a video file accepted by the storage service.
/// The caller persists <see cref="RelativePath"/> as the episode's video key and resolves it back
/// through <see cref="IVideoStorageService.OpenAsync"/>, so the physical layout never reaches the
/// browser and a storage-root move needs no database rewrite.
/// </summary>
/// <param name="RelativePath">The storage key: the protected root's server-relative directory plus file.</param>
/// <param name="SizeBytes">Size of the stored file on disk.</param>
/// <param name="Duration">Duration read from the MP4 header; the persisted episode duration (s7.7.1).</param>
public sealed record StoredVideo(
    string RelativePath,
    long SizeBytes,
    TimeSpan Duration);
