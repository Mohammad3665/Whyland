using Whyland.Shared.Dtos.Catalog.Course;
using Whyland.Shared.Filters;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetForDataTable;

/// <summary>
/// Query that returns one page of courses for the admin DataTable, driven by the server-side
/// <see cref="DataTableFilter"/> criteria plus the generic picker conditions.
/// </summary>
public sealed record GetCoursesForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<CourseListItemDto>>;
