
using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.CourseCategory.Entities;

/// <summary>
/// Many-to-many relation between courses and categories (s7.4).
/// Duplicate course/category relations are prevented by a unique composite constraint.
/// </summary>
public class CourseCategory
{
    /// <summary>Parameterless constructor required by ORMs (e.g. EF Core).</summary>
    private CourseCategory() { }

    /// <summary>
    /// Creates the link between a course and a category. The course navigation is set as well, so
    /// EF can fix up the foreign key while a new course still has no generated identifier.
    /// </summary>
    /// <param name="course">The course being linked.</param>
    /// <param name="categoryId">The category to link.</param>
    public static CourseCategory Create(CourseEntity course, CategoryId categoryId)
        => new() { Course = course, CourseId = course.Id, CategoryId = categoryId };

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
