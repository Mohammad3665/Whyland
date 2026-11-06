namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Metadata of an image accepted by the storage service.
/// The caller persists <see cref="FileName"/> and builds public URLs from the relative paths, so the
/// storage layout can change without rewriting database rows.
/// </summary>
/// <param name="FileName">The server-generated file name, used for deletion.</param>
/// <param name="RelativePath">Path of the original image, relative to the storage root.</param>
/// <param name="ThumbnailRelativePath">Path of the thumbnail, or null when none was requested.</param>
/// <param name="Width">Width of the stored image after resizing.</param>
/// <param name="Height">Height of the stored image after resizing.</param>
/// <param name="SizeBytes">Size of the stored image on disk.</param>
public sealed record StoredImage(
    string FileName,
    string RelativePath,
    string? ThumbnailRelativePath,
    int Width,
    int Height,
    long SizeBytes);
