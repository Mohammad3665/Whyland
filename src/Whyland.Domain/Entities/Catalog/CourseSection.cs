namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A logical grouping of episodes within a course (s7.6).
/// </summary>
public class CourseSection : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    #endregion
    /// <summary>
    /// The course the section belongs to.
    /// </summary>
    public long CourseId { get; set; }

    /// <summary>
    /// The course the section belongs to.
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// The section title shown in the course curriculum.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sections must be displayed in ascending DisplayOrder (s7.6).
    /// </summary>
    public int DisplayOrder { get; set; }

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
    /// The episodes grouped within this section.
    /// </summary>
    public ICollection<Episode> Episodes { get; } = [];
}
