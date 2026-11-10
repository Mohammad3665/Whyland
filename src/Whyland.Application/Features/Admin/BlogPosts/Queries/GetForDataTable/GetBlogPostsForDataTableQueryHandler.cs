using Whyland.Domain.Model.Blog.BlogPost.Errors;
using Whyland.Shared.Dtos.Blog.BlogPost;

namespace Whyland.Application.Features.Admin.BlogPosts.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetBlogPostsForDataTableQuery"/> by delegating the page lookup
/// to the Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog post query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public class GetBlogPostsForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetBlogPostsForDataTableQueryHandler> logger) :
    IQueryHandler<GetBlogPostsForDataTableQuery, DataTableResult<BlogPostListItemDto>>
{
    /// <summary>
    /// Runs the DataTable listing against the database and returns the paged result.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter and the picker conditions.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged result, or an error.</returns>
    public async Task<Result<DataTableResult<BlogPostListItemDto>>> Handle(
        GetBlogPostsForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.BlogPostQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);
            return Result<DataTableResult<BlogPostListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for BlogPosts (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<BlogPostListItemDto>>.Failure(BlogPostErrors.ListFailed());
        }
    }
}
