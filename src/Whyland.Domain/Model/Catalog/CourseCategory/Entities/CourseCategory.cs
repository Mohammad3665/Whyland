
using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.CourseCategory.Entities;

/// <summary>
/// Many-to-many relation between courses and categories (s7.4).
/// Duplicate course/category relations are prevented by a unique composite constraint.
/// </summary>
public class CourseCategory
{
    /// <summary>
    /// The course this relation links to the category.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The course this relation links to the category.
    /// </summary>
    public CourseEntity Course { get; private set; } = null!;

    /// <summary>
    /// The category this relation links to the course.
    /// </summary>
    public CategoryId CategoryId { get; private set; }

    /// <summary>
    /// The category this relation links to the course.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;
}
