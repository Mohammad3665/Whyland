namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Metadata of a general attachment file accepted by the storage service.
/// The caller persists <see cref="RelativePath"/> and <see cref="ContentType"/> along with the
/// display file name it derives itself; the storage kind and the public download name stay separate.
/// </summary>
/// <param name="RelativePath">The storage key: the protected root's server-relative directory plus file.</param>
/// <param name="SizeBytes">Size of the stored file on disk.</param>
/// <param name="ContentType">The MIME type recorded for the download response.</param>
public sealed record StoredFile(
    string RelativePath,
    long SizeBytes,
    string ContentType);
