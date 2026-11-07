namespace Whyland.Application.Features.Admin.Courses.Queries.GetFilterFields;

/// <summary>
/// Describes every course field the table can be filtered or sorted on, so the generic filter
/// picker is built from the model instead of hand-written per field.
/// </summary>
public sealed record GetCourseFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
