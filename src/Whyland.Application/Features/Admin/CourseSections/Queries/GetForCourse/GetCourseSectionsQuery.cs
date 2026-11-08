namespace Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;

/// <summary>
/// Query that returns the live sections of one course in display order, each with its episode count.
/// </summary>
/// <param name="CourseId">The course whose sections are read.</param>
public sealed record GetCourseSectionsQuery(long CourseId)
    : IQuery<IReadOnlyList<CourseSectionListItemDto>>;
