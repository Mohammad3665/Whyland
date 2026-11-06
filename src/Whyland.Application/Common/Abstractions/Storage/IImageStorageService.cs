namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Stores uploaded images in a server-managed location after validating and processing them (s8.3.1).
/// <para>
/// A stored image is always re-decoded, and optionally re-encoded, so the bytes that reach the public
/// folder never carry an unverified format or executable content. Storage keys are generated here and
/// never come from the caller.
/// </para>
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Validates the upload, writes the optimized image (and optionally its thumbnail) under
    /// <paramref name="areaKey"/> and returns the stored file names and dimensions.
    /// </summary>
    /// <param name="content">The uploaded content stream.</param>
    /// <param name="fileName">The original file name, used only to check the extension.</param>
    /// <param name="contentType">The declared content type, when the transport provides one.</param>
    /// <param name="contentLength">The declared length, re-checked against the bytes actually read.</param>
    /// <param name="areaKey">A configured storage area such as <c>Course</c>.</param>
    /// <param name="options">Per-upload processing options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored image on success, or a localized boundary error on failure.</returns>
    Task<Result<StoredImage>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        string areaKey,
        ImageStorageSaveOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a previously stored image and its thumbnail. A file that is already gone is not an error.
    /// </summary>
    /// <param name="areaKey">The storage area the image belongs to.</param>
    /// <param name="fileName">The server-generated file name returned by <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result> DeleteAsync(string areaKey, string fileName, CancellationToken ct = default);
}
