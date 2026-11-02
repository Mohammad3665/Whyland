namespace Whyland.Application.Features.Admin.Categories.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetCategoriesForDataTableQuery"/> by delegating the page lookup
/// to the Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to category query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public class GetCategoriesForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCategoriesForDataTableQueryHandler> logger) :
    IQueryHandler<GetCategoriesForDataTableQuery, DataTableResult<CategoryListItemDto>>
{
    /// <summary>
    /// Runs the DataTable listing against the database and returns the paged result.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter and the picker conditions.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged <see cref="DataTableResult{CategoryListItemDto}"/>, or an error.</returns>
    public async Task<Result<DataTableResult<CategoryListItemDto>>> Handle(GetCategoriesForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.CategoryQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);
            return Result<DataTableResult<CategoryListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for Categories (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<CategoryListItemDto>>.Failure(CategoryErrors.ListFailed());
        }
    }
}
