using Whyland.Shared.Dtos.Content.Faq;

namespace Whyland.Application.Features.Admin.Faqs.Queries.GetById;

/// <summary>
/// Handles <see cref="GetFaqByIdQuery"/> by loading the FAQ through the
/// query repository and mapping it to a <see cref="FaqDto"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ query data.</param>
public sealed class GetFaqByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetFaqByIdQuery, FaqDto>
{
    /// <summary>
    /// Loads the requested FAQ, or returns a NotFound error when it does not exist.
    /// </summary>
    /// <param name="query">The query containing the FAQ identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The mapped <see cref="FaqDto"/>, or a NotFound error.</returns>
    public async Task<Result<FaqDto>> Handle(GetFaqByIdQuery query, CancellationToken ct)
    {
        var faq = await unitOfWork.FaqQueryRepository.GetByIdAsync(query.Id, ct);
        if(faq is null)
            return FaqErrors.NotFound(query.Id);

        return faq.Adapt<FaqDto>();
    }
}