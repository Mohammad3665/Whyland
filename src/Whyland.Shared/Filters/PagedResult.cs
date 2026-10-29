namespace Whyland.Shared.Filters;

/// <summary>
/// One page of rows plus the number of rows the same filters match in total. This is the
/// envelope for ordinary paged reads; the DataTables envelope is <see cref="DataTableResult{T}"/>,
/// which additionally carries the draw counter and the unfiltered record count.
/// </summary>
/// <typeparam name="T">The row type.</typeparam>
/// <param name="Items">The rows of this page, in the requested order.</param>
/// <param name="Page">The one-based page that was read.</param>
/// <param name="PageSize">The page size that was applied.</param>
/// <param name="TotalCount">How many rows match the filters, across all pages.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);
