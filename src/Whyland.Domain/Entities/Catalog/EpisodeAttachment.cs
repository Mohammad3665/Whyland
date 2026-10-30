namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A downloadable attachment of an episode (s7.8).
/// Count/size/extension limits are configuration-driven and enforced server-side;
/// file metadata is persisted while the binary lives in protected wwwroot storage (s7.8, s8.1).
/// </summary>
public class EpisodeAttachment : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="ContentType"/>; also the database column size.</summary>
    public const int MaxContentTypeLength = 128;

    /// <summary>Maximum allowed length of <see cref="FileName"/>; also the database column size.</summary>
    public const int MaxFileNameLength = 256;

    /// <summary>Maximum allowed length of <see cref="StoragePath"/>; also the database column size.</summary>
    public const int MaxStoragePathLength = 512;

    #endregion
    /// <summary>
    /// The episode the attachment belongs to.
    /// </summary>
    public long EpisodeId { get; set; }

    /// <summary>
    /// The episode the attachment belongs to.
    /// </summary>
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

    /// <summary>
    /// The MIME content type used when serving the download.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size in bytes; validated against the configured maximum.
    /// </summary>
    public long Size { get; set; }
}
