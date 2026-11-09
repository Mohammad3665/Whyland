namespace Whyland.Application.Features.Media.EpisodeAttachments;

/// <summary>
/// An authorized attachment download: the open file plus the name and media type the response
/// headers must carry. The file name is the sanitized display name stored with the row, never the
/// storage key.
/// </summary>
/// <param name="Content">The open, read-only file stream; the response disposes it.</param>
/// <param name="FileName">The download name shown to the user.</param>
/// <param name="ContentType">The MIME type recorded for the file.</param>
public sealed record EpisodeAttachmentFile(
    Stream Content,
    string FileName,
    string ContentType);
