namespace Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;

/// <summary>
/// Query that returns the live FAQ items of one course in display order.
/// </summary>
/// <param name="CourseId">The course whose questions are read.</param>
public sealed record GetCourseFaqsQuery(CourseId CourseId)
    : IQuery<IReadOnlyList<CourseFaqListItemDto>>;
