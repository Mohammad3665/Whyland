namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A logical grouping of episodes within a course (s7.6).
/// </summary>
public class CourseSection : BaseEntity<long>
{
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
    /// The episodes grouped within this section.
    /// </summary>
    public ICollection<Episode> Episodes { get; set; } = [];
}
