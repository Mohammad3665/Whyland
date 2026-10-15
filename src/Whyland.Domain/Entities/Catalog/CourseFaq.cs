
namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<long>
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
