
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Entities;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<CourseFaqId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    #endregion
    /// <summary>
    /// The course the FAQ item belongs to.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The course the FAQ item belongs to.
    /// </summary>
    public CourseEntity Course { get; private set; } = null!;

    /// <summary>
    /// The question text.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; items are shown in this order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public UserId? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }
}
