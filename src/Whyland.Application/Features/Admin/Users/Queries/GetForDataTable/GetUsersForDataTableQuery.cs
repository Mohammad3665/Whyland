namespace Whyland.Application.Features.Admin.Users.Queries.GetForDataTable;

/// <summary>
/// Reads the admin users table for the given paging/sorting/search parameters plus any
/// number of generic field/operator/value conditions. No per-field filtering code exists:
/// every condition is resolved against the account properties by the shared pipeline.
/// </summary>
/// <param name="Filter">Paging, sorting and the field the free-text search applies to.</param>
/// <param name="Filters">Extra conditions ANDed with the search.</param>
public sealed record GetUsersForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<UserListItemDto>>;
