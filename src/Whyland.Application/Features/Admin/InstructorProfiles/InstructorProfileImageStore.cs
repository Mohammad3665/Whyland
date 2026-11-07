using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.InstructorProfiles;

/// <summary>
/// The one place that knows where instructor images live and how a stored path becomes a URL.
/// </summary>
public static class InstructorProfileImageStore
{
    /// <summary>Logical area configured for instructor images under <c>FileStorage:Areas</c>.</summary>
    public const string AreaKey = "Instructor";

    /// <summary>Public prefix the storage root is served under.</summary>
    public const string PublicRoot = "/uploads/images/";

    /// <summary>Directory segment the full-size image is stored in.</summary>
    private const string OriginalSegment = "/originals/";

    /// <summary>Stores one image, re-encoded and optimized by the storage service.</summary>
    public static Task<Result<StoredImage>> SaveAsync(
        IImageStorageService storage,
        InstructorProfileImageUpload upload,
        CancellationToken ct = default)
        => storage.SaveAsync(
            upload.Content,
            upload.FileName,
            upload.ContentType,
            upload.Length,
            AreaKey,
            new ImageStorageSaveOptions
            {
                GenerateThumbnail = false,
                OptimizeImage = true,
                ConvertToWebP = true,
            },
            ct);

    /// <summary>Deletes the file behind a stored path; foreign paths are ignored.</summary>
    public static Task<Result> DeleteAsync(
        IImageStorageService storage,
        string? storedPath,
        CancellationToken ct = default)
    {
        var fileName = FileNameOf(storedPath);
        return fileName is null
            ? Task.FromResult(Result.Success())
            : storage.DeleteAsync(AreaKey, fileName, ct);
    }

    /// <summary>Builds the public URL of a stored path, or null when there is no image.</summary>
    public static string? ToPublicUrl(string? storedPath)
        => string.IsNullOrWhiteSpace(storedPath)
            ? null
            : PublicRoot + storedPath.Trim().TrimStart('/');

    /// <summary>Returns the stored file name when the path belongs to the instructor image area.</summary>
    public static string? FileNameOf(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return null;

        var normalized = storedPath.Trim().Replace('\\', '/').TrimStart('/');
        if (!normalized.StartsWith("instructors/originals/", StringComparison.Ordinal))
            return null;

        var fileName = Path.GetFileName(normalized);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }

    /// <summary>Returns the stored original path, which is what the entity column keeps.</summary>
    public static string? RelativePathOf(string? storedPath)
        => string.IsNullOrWhiteSpace(storedPath)
            ? null
            : storedPath.Trim().TrimStart('/');

    /// <summary>Whether a stored path is under the instructor originals directory.</summary>
    public static bool IsOwnedPath(string? storedPath)
        => !string.IsNullOrWhiteSpace(storedPath) &&
           storedPath.Trim().Replace('\\', '/').TrimStart('/')
               .StartsWith("instructors/originals/", StringComparison.Ordinal);

    /// <summary>The directory segment used by the store; kept for callers that compare paths.</summary>
    public static string DirectorySegment => OriginalSegment.Trim('/');
}
