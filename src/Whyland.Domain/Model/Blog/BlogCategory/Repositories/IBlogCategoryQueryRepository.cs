using Whyland.Shared.Dtos.Blog.BlogCategory;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Blog.BlogCategory.Repositories;

/// <summary>
/// Query-side repository for the BlogCategory aggregate.
/// Implemented with Dapper for lightweight, non-tracked reads.
/// </summary>
public interface IBlogCategoryQueryRepository
{
    /// <summary>
    /// Returns one page of blog categories for the admin DataTables listing. Every row carries its
    /// parent name, the level it sits on, how many live posts reference it and the audit columns,
    /// which is all the table and the details modal need.
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="conditions">
    /// Generic field/operator/value conditions from the filter picker, ANDed together. Each field
    /// name is resolved against the spec's column whitelist, so an unknown one is dropped instead
    /// of reaching the SQL.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<DataTableResult<BlogCategoryListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);
}