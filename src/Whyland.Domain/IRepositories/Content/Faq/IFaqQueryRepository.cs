using Whyland.Shared.Dtos.Content.Faq;
using Whyland.Shared.Filters;

namespace Whyland.Domain.IRepositories.Content.Faq;

/// <summary>
/// Query-side repository for the Faq aggregate.
/// Implemented with Dapper for lightweight, non-tracked read queries.
/// </summary>
public interface IFaqQueryRepository
{
    /// <summary>
    /// Returns one FAQ with its full details (including audit fields).
    /// </summary>
    /// <param name="id">Faq identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The Faq DTO, or null if not found.</returns>
    Task<FaqDto?> GetByIdAsync(long id, CancellationToken ct = default);
    
    /// <summary>
    /// Returns one page of rows for the admin DataTables listing
    /// (server-side pagination, ordering and search).
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged result of Faq list items.</returns>
    Task<DataTableResult<FaqListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all active FAQs ordered by DisplayOrder (ascending),
    /// for the public site FAQ section.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of active FAQs ordered by DisplayOrder.</returns>
    Task<IReadOnlyList<FaqPublicItemDto>> GetActiveListAsync(CancellationToken ct = default);
}