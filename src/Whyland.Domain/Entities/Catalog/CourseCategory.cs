namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// Many-to-many relation between courses and categories (s7.4).
/// Duplicate course/category relations are prevented by a unique composite constraint.
/// </summary>
public class CourseCategory
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public long CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}
