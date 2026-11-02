namespace Whyland.Application.Features.Admin.Categories.Queries.GetForDataTable;

/// <summary>
/// Query that returns one page of categories for the admin DataTable listing,
/// driven by the server-side <see cref="DataTableFilter"/> criteria.
/// </summary>
/// <param name="Filter">Pagination, ordering and search criteria sent by the DataTable.</param>
/// <param name="Filters">
/// Generic field/operator/value conditions built by the filter picker; ANDed together and
/// resolved against the same column whitelist the listing reads from.
/// </param>
public sealed record GetCategoriesForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<CategoryListItemDto>>;
