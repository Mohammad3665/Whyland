namespace Whyland.Application.Features.Admin.Faqs.Queries.GetActives;

/// <summary>
/// Handles <see cref="GetActiveFaqsQuery"/> by loading the active FAQs through
/// the query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ query data.</param>
public sealed class GetActiveFaqsQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetActiveFaqsQuery, IReadOnlyList<FaqPublicItemDto>>
{
    /// <summary>
    /// Returns the active FAQs ordered by DisplayOrder, or an error when the
    /// lookup fails.
    /// </summary>
    /// <param name="query">The query selecting the active FAQ list.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The read-only list of active FAQs, or an error.</returns>
    public async Task<Result<IReadOnlyList<FaqPublicItemDto>>> Handle(GetActiveFaqsQuery query, CancellationToken ct)
    {
        var faqs = await unitOfWork.FaqQueryRepository.GetActiveListAsync(ct);
        return Result<IReadOnlyList<FaqPublicItemDto>>.Success(faqs);
    }
}
