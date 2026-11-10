namespace Whyland.Application.Features.Admin.BlogCategories.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetBlogCategoriesForDataTableQuery"/> by delegating the page lookup
/// to the Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog category query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public class GetBlogCategoriesForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetBlogCategoriesForDataTableQueryHandler> logger) :
    IQueryHandler<GetBlogCategoriesForDataTableQuery, DataTableResult<BlogCategoryListItemDto>>
{
    /// <summary>
    /// Runs the DataTable listing against the database and returns the paged result.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter and the picker conditions.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged <see cref="DataTableResult{BlogCategoryListItemDto}"/>, or an error.</returns>
    public async Task<Result<DataTableResult<BlogCategoryListItemDto>>> Handle(
        GetBlogCategoriesForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.BlogCategoryQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);
            return Result<DataTableResult<BlogCategoryListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for BlogCategories (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<BlogCategoryListItemDto>>.Failure(BlogCategoryErrors.ListFailed());
        }
    }
}