namespace Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;

/// <summary>Returns the live FAQs of one course in display order.</summary>
public sealed record GetCourseFaqsQuery(long CourseId)
    : IQuery<IReadOnlyList<CourseFaqListItemDto>>;
