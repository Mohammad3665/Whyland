namespace Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;

/// <summary>Returns the live sections of one course in display order.</summary>
public sealed record GetCourseSectionsQuery(long CourseId)
    : IQuery<IReadOnlyList<CourseSectionListItemDto>>;
