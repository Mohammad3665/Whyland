namespace Whyland.Application.Features.Admin.EpisodeAttachments;

/// <summary>
/// One uploaded attachment file handed to the application layer. The page model opens the form-file
/// stream and disposes it after the dispatch returns; the command and its handler never see ASP.NET
/// types.
/// </summary>
/// <param name="Content">The uploaded bytes.</param>
/// <param name="FileName">Original file name, used for the extension check and the display name.</param>
/// <param name="ContentType">Declared content type, when the transport provides one.</param>
/// <param name="Length">Declared length, re-checked against the bytes actually read.</param>
public sealed record EpisodeAttachmentUpload(
    Stream Content,
    string FileName,
    string? ContentType,
    long Length);
