namespace Whyland.Application.Features.Admin.Faqs.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetFaqsForDataTableQuery"/> by delegating the page lookup
/// to the Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public class GetFaqsForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetFaqsForDataTableQueryHandler> logger) :
    IQueryHandler<GetFaqsForDataTableQuery, DataTableResult<FaqListItemDto>>
{
    /// <summary>
    /// Runs the DataTable listing against the database and returns the paged result.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged <see cref="DataTableResult{FaqListItemDto}"/>, or an error.</returns>
    public async Task<Result<DataTableResult<FaqListItemDto>>> Handle(GetFaqsForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.FaqQueryRepository.GetForDataTableAsync(query.Filter, ct);
            return Result<DataTableResult<FaqListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for FAQs (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<FaqListItemDto>>.Failure(FaqErrors.ListFailed());
        }
    }
}
