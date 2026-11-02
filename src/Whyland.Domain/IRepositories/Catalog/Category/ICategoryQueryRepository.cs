using Whyland.Shared.Dtos.Catalog.Category;
using Whyland.Shared.Filters;

namespace Whyland.Domain.IRepositories.Catalog.Category;

/// <summary>
/// Query-side repository for the Category aggregate.
/// Implemented with Dapper for lightweight, non-tracked reads.
/// </summary>
public interface ICategoryQueryRepository
{
    /// <summary>
    /// Returns one page of categories for the admin DataTables listing. Every row carries its parent
    /// name, the level it sits on and the audit columns, which is all the table and the details
    /// modal need.
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="conditions">
    /// Generic field/operator/value conditions from the filter picker, ANDed together. Each field
    /// name is resolved against the spec's column whitelist, so an unknown one is dropped instead
    /// of reaching the SQL.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<DataTableResult<CategoryListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);
}
