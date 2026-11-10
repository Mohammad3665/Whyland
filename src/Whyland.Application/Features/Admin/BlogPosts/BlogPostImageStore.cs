using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.BlogPosts;

/// <summary>
/// The one place that knows where blog post images live and how a stored path becomes a public URL.
/// <para>
/// The row stores the relative path returned by the storage service, never a public URL: moving the
/// storage root or the served prefix would otherwise require rewriting rows, and the thumbnail path
/// can be derived from the original one without another column.
/// </para>
/// </summary>
public static class BlogPostImageStore
{
    /// <summary>Logical area configured for blog images under <c>FileStorage:Areas</c>.</summary>
    public const string AreaKey = "Blog";

    /// <summary>Public prefix the storage root is served under.</summary>
    public const string PublicRoot = "/uploads/images/";

    /// <summary>Directory segment the full-size image is stored in.</summary>
    private const string OriginalSegment = "/originals/";

    /// <summary>Directory segment the thumbnail is stored in.</summary>
    private const string ThumbnailSegment = "/thumbnails/";

    /// <summary>
    /// Stores one uploaded cover image with a thumbnail. Validation, re-encoding and local
    /// failures belong to the storage service, so its error is returned unchanged.
    /// </summary>
    public static Task<Result<StoredImage>> SaveAsync(
        IImageStorageService storage,
        BlogPostImageUpload upload,
        CancellationToken ct = default)
        => storage.SaveAsync(
            upload.Content,
            upload.FileName,
            upload.ContentType,
            upload.Length,
            AreaKey,
            new ImageStorageSaveOptions
            {
                GenerateThumbnail = true,
                OptimizeImage = true,
                ConvertToWebP = true,
            },
            ct);

    /// <summary>
    /// Deletes the file behind a stored relative path. Only paths this store owns are touched;
    /// anything else is ignored.
    /// </summary>
    public static Task<Result> DeleteAsync(
        IImageStorageService storage,
        string? storedPath,
        CancellationToken ct = default)
    {
        var fileName = FileNameOf(storedPath);
        if (fileName is null)
            return Task.FromResult(Result.Success());

        return storage.DeleteAsync(AreaKey, fileName, ct);
    }

    /// <summary>Builds the public URL of a stored relative path, or null when there is no image.</summary>
    public static string? ToPublicUrl(string? storedPath)
        => string.IsNullOrWhiteSpace(storedPath)
            ? null
            : PublicRoot + storedPath.Trim().TrimStart('/');

    /// <summary>
    /// Builds the public URL of the thumbnail next to a stored original. The storage service writes
    /// both under the same file name, so only the directory segment changes.
    /// </summary>
    public static string? ToThumbnailUrl(string? storedPath)
        => ToPublicUrl(storedPath)?.Replace(OriginalSegment, ThumbnailSegment, StringComparison.Ordinal);

    /// <summary>Returns the stored file name when the path belongs to the blog image area.</summary>
    public static string? FileNameOf(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return null;

        var normalized = storedPath.Trim().Replace('\\', '/').TrimStart('/');

        // The service names files with a GUID and one of its output extensions; requiring the blog
        // directory keeps an arbitrary value in the column from reaching the disk.
        if (!normalized.StartsWith("blogs/originals/", StringComparison.Ordinal))
            return null;

        var fileName = Path.GetFileName(normalized);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }
}
