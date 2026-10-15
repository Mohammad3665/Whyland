
namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A logical grouping of episodes within a course (s7.6).
/// </summary>
public class CourseSection : BaseEntity<long>
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sections must be displayed in ascending DisplayOrder (s7.6).
    /// </summary>
    public int DisplayOrder { get; set; }

    public ICollection<Episode> Episodes { get; set; } = [];
}
