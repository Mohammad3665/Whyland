using Whyland.Application.Common.Abstractions.CQRS.Query;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetById;

/// <summary>Query that returns the editable course with its selected links.</summary>
/// <param name="Id">The course identifier.</param>
public sealed record GetCourseByIdQuery(long Id) : IQuery<CourseDetailsDto>;
