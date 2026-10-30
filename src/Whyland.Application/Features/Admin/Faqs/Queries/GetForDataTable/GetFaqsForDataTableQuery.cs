namespace Whyland.Application.Features.Admin.Faqs.Queries.GetForDataTable;

/// <summary>
/// Query that returns one page of FAQs for the admin DataTable listing,
/// driven by the server-side <see cref="DataTableFilter"/> criteria.
/// </summary>
/// <param name="Filter">Pagination, ordering and search criteria sent by the DataTable.</param>
public sealed record GetFaqsForDataTableQuery(DataTableFilter Filter) : IQuery<DataTableResult<FaqListItemDto>>;
