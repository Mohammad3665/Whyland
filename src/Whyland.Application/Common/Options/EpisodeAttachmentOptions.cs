namespace Whyland.Application.Common.Options;

/// <summary>
/// Limits and layout of the protected episode-attachment storage (s7.8, s8.3.1).
/// Bound from the <c>EpisodeAttachment</c> configuration section; the same numbers back the client
/// hints the upload modal shows and the server checks, so both sides always agree.
/// </summary>
public sealed class EpisodeAttachmentOptions
{
    /// <summary>Configuration section this class binds from.</summary>
    public const string SectionName = "EpisodeAttachment";

    /// <summary>
    /// Root of the protected media storage, relative to the content root. Static serving of this
    /// root is blocked, so a download is only ever reachable through the checked endpoint (s8.2.1).
    /// </summary>
    public string RootPath { get; set; } = "wwwroot/uploads/media";

    /// <summary>Directory segment the attachment files are stored in, inside the root.</summary>
    public string AttachmentDirectory { get; set; } = "files";

    /// <summary>The largest single attachment, in bytes (100 MB by default).</summary>
    public long MaxAttachmentSizeBytes { get; set; } = 100L * 1024 * 1024;

    /// <summary>How many live attachments one episode may hold (s7.8).</summary>
    public int MaxAttachmentsPerEpisode { get; set; } = 10;

    /// <summary>
    /// The extensions an attachment may carry, lowercase with the leading dot. SVG is deliberately
    /// absent: script-bearing documents must not be accepted by default (s8.3.1).
    /// </summary>
    public IReadOnlyList<string> AllowedExtensions { get; set; } = new[]
    {
        ".pdf", ".zip", ".rar", ".7z",
        ".doc", ".docx", ".xls", ".xlsx",
        ".ppt", ".pptx", ".txt", ".mp3",
    };
}
