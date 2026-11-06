using Whyland.Shared.Dtos.Authorization.Role;

namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetForDataTable;

/// <summary>
/// Reads the admin roles table for the given paging/sorting/search parameters plus any number of
/// generic field/operator/value conditions. No per-field filtering code exists: every condition is
/// resolved against the whitelisted role columns by the shared pipeline, the two counted columns
/// included.
/// </summary>
/// <param name="Filter">Paging, sorting and the field the free-text search applies to.</param>
/// <param name="Filters">Extra conditions ANDed with the search.</param>
public sealed record GetRolesForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<RoleListItemDto>>;
