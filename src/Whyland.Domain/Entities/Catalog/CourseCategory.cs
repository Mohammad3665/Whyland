namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// Many-to-many relation between courses and categories (s7.4).
/// Duplicate course/category relations are prevented by a unique composite constraint.
/// </summary>
public class CourseCategory
{
    /// <summary>
    /// The course this relation links to the category.
    /// </summary>
    public long CourseId { get; set; }

    /// <summary>
    /// The course this relation links to the category.
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// The category this relation links to the course.
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// The category this relation links to the course.
    /// </summary>
    public Category Category { get; set; } = null!;
}
