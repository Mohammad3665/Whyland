namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<long>
{
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
