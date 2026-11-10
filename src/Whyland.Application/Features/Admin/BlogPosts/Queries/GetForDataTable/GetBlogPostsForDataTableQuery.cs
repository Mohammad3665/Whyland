using Whyland.Shared.Dtos.Blog.BlogPost;

namespace Whyland.Application.Features.Admin.BlogPosts.Queries.GetForDataTable;

/// <summary>
/// Query that returns one page of blog posts for the admin DataTable listing,
/// driven by the server-side <see cref="DataTableFilter"/> criteria.
/// </summary>
/// <param name="Filter">Pagination, ordering and search criteria sent by the DataTable.</param>
/// <param name="Filters">
/// Generic field/operator/value conditions built by the filter picker; ANDed together and
/// resolved against the same column whitelist the listing reads from.
/// </param>
public sealed record GetBlogPostsForDataTableQuery(
    DataTableFilter Filter,
    IReadOnlyList<FilterCondition>? Filters = null) : IQuery<DataTableResult<BlogPostListItemDto>>;
