namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<long>, ISoftDeletable
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
    public long CourseId { get; set; }

    /// <summary>
    /// The course the FAQ item belongs to.
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// The question text.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; items are shown in this order.
    /// </summary>
    public int DisplayOrder { get; set; }
}
