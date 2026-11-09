namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Stores general attachment documents (PDFs, archives, ...) on local disk after validating the
/// extension, the size and where possible the binary signature (s7.8, s8.3.1).
/// <para>
/// Files live in the protected media root whose static serving is blocked; downloads are served
/// through the controlled endpoint that checks course access first. Storage keys are generated
/// here and never sent by the caller.
/// </para>
/// </summary>
public interface IAttachmentStorageService
{
    /// <summary>
    /// Validates the upload and stores it under the attachment area. The stored name is generated
    /// server-side; the display name the caller keeps in the database is derived from
    /// <paramref name="fileName"/> by <see cref="SanitizeFileName"/>.
    /// </summary>
    /// <param name="content">The uploaded content stream.</param>
    /// <param name="fileName">The original file name, used to check the extension and the display name.</param>
    /// <param name="contentType">The declared content type, when the transport provides one.</param>
    /// <param name="contentLength">The declared length, re-checked against the bytes actually read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored file, or the stable storage error on rejection.</returns>
    Task<Result<StoredFile>> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        long contentLength,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a previously stored attachment. A file that is already gone is not an error.
    /// </summary>
    /// <param name="storagePath">The server-generated relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result> DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Opens a stored attachment for streaming. The caller closes the returned stream when done, and
    /// is responsible for having authorized the request before opening it.
    /// </summary>
    /// <param name="storagePath">The server-generated relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result<VideoFileStream>> OpenAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Reduces a client-sent file name to the safe display form: path segments stripped, control
    /// characters removed, whitespace trimmed. Empty input yields the empty string.
    /// </summary>
    /// <param name="fileName">The file name as advertised by the browser.</param>
    /// <returns>The sanitized display name, without any directory information.</returns>
    string SanitizeFileName(string? fileName);
}
