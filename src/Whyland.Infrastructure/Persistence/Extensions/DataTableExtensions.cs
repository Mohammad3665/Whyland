using Mapster;
using Whyland.Application.Common.Extensions;

namespace Whyland.Infrastructure.Persistence.Extensions;

/// <summary>
/// Extension methods for converting IQueryable collections to DataTableResult format.
/// Provides server-side paging, sorting, and filtering support for DataTables.
/// Lives in Infrastructure because it composes EF Core query execution with Mapster projection;
/// callers are repository/query handlers, never the Domain layer.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Converts an IQueryable to a DataTableResult with server-side paging and sorting.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="filter">The DataTable filter containing paging and sorting parameters.</param>
    /// <param name="extraFilters">Additional field/operator/value conditions, ANDed with the search.</param>
    /// <returns>A DataTableResult with paginated and sorted data.</returns>
    public static async Task<DataTableResult<TEntity>> ToDataTableAsync<TEntity>(
        this IQueryable<TEntity> source,
        DataTableFilter filter,
        IEnumerable<FilterCondition>? extraFilters = null)
    {
        return await source.ToDataTableAsync<TEntity, TEntity>(filter, extraFilters);
    }

    /// <summary>
    /// Converts an IQueryable to a DataTableResult with projection to a different type.
    /// Applies the selector before paging for better performance.
    /// </summary>
    /// <typeparam name="TEntity">The source entity type.</typeparam>
    /// <typeparam name="TResult">The result DTO type.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="filter">The DataTable filter containing paging and sorting parameters.</param>
    /// <param name="extraFilters">Additional field/operator/value conditions, ANDed with the search.</param>
    /// <returns>A DataTableResult with paginated, sorted, and projected data.</returns>
    public static async Task<DataTableResult<TResult>> ToDataTableAsync<TEntity, TResult>(
        this IQueryable<TEntity> source,
        DataTableFilter filter,
        IEnumerable<FilterCondition>? extraFilters = null)
    {
        var recordsTotal = await source.CountAsync();

        // The generic search box and the field/operator/value pickers both become
        // FilterConditions, so a list page never needs its own per-field filter code.
        var queryRequest = filter.ToQueryRequest<TEntity>();
        if (extraFilters is not null)
            (queryRequest.Filters ??= []).AddRange(extraFilters);

        var filteredQuery = source.ApplyFilters(queryRequest.Filters);
        var recordsFiltered = await filteredQuery.CountAsync();
        ConfigPaging(ref filter, recordsFiltered);

        var sortedQuery = filteredQuery.ApplySorting(queryRequest.SortBy, queryRequest.Sort);

        var data = await sortedQuery
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ProjectToType<TResult>()
            .ToListAsync();

        return new DataTableResult<TResult>
        {
            Draw = filter.Draw ?? "1",
            RecordsFiltered = recordsFiltered,
            RecordsTotal = recordsTotal,
            Data = data
        };
    }

    /// <summary>
    /// Configures paging parameters when "show all" is requested (PageSize = -1)
    /// and normalizes a missing/zero PageSize to the default page size, so paging
    /// never silently returns an empty dataset.
    /// </summary>
    /// <param name="filter">The filter to configure.</param>
    /// <param name="recordsTotal">Total number of records.</param>
    private static void ConfigPaging(ref DataTableFilter filter, int recordsTotal)
    {
        // DataTables sends paging as strings; normalize them here so every caller gets a
        // usable PageSize/Skip pair instead of having to parse them per page.
        if (filter.PageSize == 0 && int.TryParse(filter.Length, out var length))
            filter.PageSize = length;
        if (filter.Skip == 0 && int.TryParse(filter.Start, out var start))
            filter.Skip = start;

        if (filter.PageSize == -1)
        {
            filter.PageSize = recordsTotal;
            filter.Skip = 0;
        }
        else if (filter.PageSize <= 0)
        {
            // Default page size, mirroring ToQueryRequest's fallback
            filter.PageSize = 10;
        }
    }
}
