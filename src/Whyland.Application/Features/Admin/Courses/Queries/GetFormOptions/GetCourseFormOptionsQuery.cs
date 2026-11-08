using Whyland.Shared.Dtos.Catalog.Course;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetFormOptions;

/// <summary>
/// Query that returns the active categories and the instructors the course form and its explicit
/// filters select from. The listing scope is fixed by the repository, so the query itself carries
/// no parameters.
/// </summary>
public sealed record GetCourseFormOptionsQuery : IQuery<CourseFormOptionsDto>;
