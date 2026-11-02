namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// An individual educational lesson within a section (s7.7).
/// Note: per-user watch status is NOT stored here; it lives in a dedicated
/// per-user progress entity (s7.7.4).
/// </summary>
public class Episode : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum allowed length of <see cref="Video"/>; also the database column size.</summary>
    public const int MaxVideoLength = 512;

    #endregion
    /// <summary>
    /// The section the episode belongs to.
    /// </summary>
    public long SectionId { get; private set; }

    /// <summary>
    /// The section the episode belongs to.
    /// </summary>
    public CourseSection Section { get; private set; } = null!;

    /// <summary>
    /// The episode title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Optional short description of the episode's content.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Reference to the stored MP4 video; optional/required per episode kind.
    /// Served through a permission-checked, controlled endpoint (s8.1).
    /// </summary>
    public string? Video { get; private set; }

    /// <summary>
    /// Required when a video exists.
    /// </summary>
    public TimeSpan? Duration { get; private set; }

    /// <summary>
    /// A paid course can expose episodes as free previews (s7.7.3).
    /// Free preview access must not grant ownership of the whole course.
    /// </summary>
    public bool IsFree { get; private set; }

    /// <summary>
    /// Manual ordering weight; episodes are shown in ascending order within the section.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public Guid? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// The downloadable attachments of this episode.
    /// </summary>
    public ICollection<EpisodeAttachment> Attachments { get; } = [];
}
