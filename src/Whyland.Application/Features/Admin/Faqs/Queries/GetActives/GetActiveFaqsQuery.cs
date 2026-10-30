namespace Whyland.Application.Features.Admin.Faqs.Queries.GetActives;

/// <summary>
/// Query that returns all active FAQs, for the public site FAQ section (s4.12).
/// The listing scope is fixed by the repository (IsActive, not soft-deleted),
/// so the query itself carries no parameters.
/// </summary>
public sealed record GetActiveFaqsQuery() : IQuery<IReadOnlyList<FaqPublicItemDto>>;
