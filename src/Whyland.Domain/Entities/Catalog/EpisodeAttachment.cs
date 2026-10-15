
namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A downloadable attachment of an episode (s7.8).
/// Count/size/extension limits are configuration-driven and enforced server-side;
/// file metadata is persisted while the binary lives in protected wwwroot storage (s7.8, s8.1).
/// </summary>
public class EpisodeAttachment : BaseEntity<long>
{
    public long EpisodeId { get; set; }

    public Episode Episode { get; set; } = null!;

    /// <summary>
    /// The display/download file name shown to the user.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Server-managed storage reference under protected wwwroot. Must never expose
    /// physical paths or allow arbitrary path traversal (s8.1.1, s8.5).
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size in bytes; validated against the configured maximum.
    /// </summary>
    public long Size { get; set; }
}
