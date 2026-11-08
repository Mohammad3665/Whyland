using Whyland.Shared.Dtos.Catalog.Course;
using Whyland.Shared.Filters;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetForDataTable;

/// <summary>
/// Query that returns one page of courses for the admin DataTable, driven by the server-side
/// <see cref="DataTableFilter"/> criteria plus the generic picker conditions.
/// </summary>
/// <param name="Filter">Pagination, ordering and search criteria sent by the DataTable.</param>
/// <param name="Filters">Field/operator/value conditions sent by the generic filter picker.</param>
public sealed record GetCoursesForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<CourseListItemDto>>;
