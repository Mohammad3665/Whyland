namespace Whyland.Shared.Dtos.Catalog.Course;

/// <summary>
/// One active category in the course form and in the category filter. The parent id and the
/// ordering weight let the client render the same indented tree the category page shows.
/// </summary>
/// <param name="Id">Category identifier.</param>
/// <param name="Name">Category name.</param>
/// <param name="ParentId">Parent category, or null for a root.</param>
/// <param name="DisplayOrder">Position inside its sibling group.</param>
public sealed record CourseCategoryOptionDto(
    long Id,
    string Name,
    long? ParentId,
    int DisplayOrder);

/// <summary>
/// One instructor in the course form and in the instructor filter.
/// </summary>
/// <param name="Id">Instructor profile identifier.</param>
/// <param name="FirstName">Account first name.</param>
/// <param name="LastName">Account last name.</param>
/// <param name="Specialization">The instructor's area of expertise, when set.</param>
public sealed record CourseInstructorOptionDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Specialization);

/// <summary>
/// The two option lists the course form and its explicit filters need.
/// </summary>
/// <param name="Categories">Active categories, ordered by display order.</param>
/// <param name="Instructors">Instructors with active accounts, ordered by name.</param>
public sealed record CourseFormOptionsDto(
    IReadOnlyList<CourseCategoryOptionDto> Categories,
    IReadOnlyList<CourseInstructorOptionDto> Instructors);
