using Whyland.Shared.Dtos.Content.Faq;

namespace Whyland.Application.Features.Admin.Faqs.Queries.GetById;

/// <summary>
/// Query that retrieves a single FAQ by its identifier and returns it as a <see cref="FaqDto"/>.
/// </summary>
/// <param name="Id">The identifier of the FAQ to retrieve.</param>
public sealed record GetFaqByIdQuery(long Id) : IQuery<FaqDto>;