namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetForDataTable;

/// <summary>Returns one page of instructor profiles for the admin DataTable.</summary>
public sealed record GetInstructorProfilesForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null)
    : IQuery<DataTableResult<InstructorProfileListItemDto>>;
