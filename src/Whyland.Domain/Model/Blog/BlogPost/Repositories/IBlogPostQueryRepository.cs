using Whyland.Shared.Dtos.Blog.BlogPost;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Blog.BlogPost.Repositories;

/// <summary>
/// Query-side repository for the BlogPost aggregate.
/// Implemented with Dapper for lightweight, non-tracked reads.
/// </summary>
public interface IBlogPostQueryRepository
{
    /// <summary>
    /// Returns one page of blog posts for the admin DataTables listing. Every row carries its author
    /// and category names, the stored image path and its public URLs, the audit columns and the
    /// article body, which is all the table, the edit form and the details modal need.
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="conditions">
    /// Generic field/operator/value conditions from the filter picker, ANDed together. Each field
    /// name is resolved against the spec's column whitelist, so an unknown one is dropped instead
    /// of reaching the SQL.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<DataTableResult<BlogPostListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);
}
