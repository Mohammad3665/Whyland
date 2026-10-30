namespace Whyland.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for converting DataTableFilter to QueryRequest.
/// Handles pagination, sorting, and search filtering logic.
/// </summary>
public static class QueryRequestMappingExtensions
{
    /// <summary>
    /// Converts a <see cref="DataTableFilter"/> object to a <see cref="QueryRequest"/> object
    /// for database querying. The conversion includes:
    /// 1. Pagination: calculates the page number from Skip and PageSize.
    /// 2. Sorting: determines the sort direction (ascending/descending).
    /// 3. Search: adds a search condition when SearchValue is provided, preferring the
    ///    specific SearchField when valid and falling back to a default field
    ///    (Name → Title → first string property → first property).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to apply filtering on.</typeparam>
    /// <param name="filter">The DataTableFilter containing client-side filtering parameters.</param>
    /// <returns>A QueryRequest object with pagination, sorting, and filter conditions.</returns>
    public static QueryRequest ToQueryRequest<TEntity>(this DataTableFilter filter)
    {
        var request = new QueryRequest
        {
            Page = filter.Skip > 0 && filter.PageSize > 0
                ? (filter.Skip / filter.PageSize) + 1
                : 1,
            Take = filter.PageSize > 0 ? filter.PageSize : 10,
            SortBy = filter.SortColumn,
            Sort = string.Equals(filter.SortColumnDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? SortType.Desc
                : SortType.Asc,
            Filters = []
        };

        if (!string.IsNullOrWhiteSpace(filter.SearchValue))
        {
            var searchValue = filter.SearchValue.Trim();

            if (!string.IsNullOrWhiteSpace(filter.SearchField) &&
                PropertyExists<TEntity>(filter.SearchField))
            {
                request.Filters.Add(new FilterCondition
                {
                    Property = filter.SearchField,
                    Operator = FilterOperator.Contains,
                    Value = searchValue
                });
            }
            else
            {
                request.Filters.Add(new FilterCondition
                {
                    Property = GetDefaultField<TEntity>(),
                    Operator = FilterOperator.Contains,
                    Value = searchValue
                });
            }
        }

        return request;
    }

    /// <summary>
    /// Checks whether a property exists on the specified entity type.
    /// </summary>
    private static bool PropertyExists<TEntity>(string propertyName)
    {
        return typeof(TEntity).GetProperty(
            propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null;
    }

    /// <summary>
    /// Gets the default search field for the specified entity type.
    /// </summary>
    private static string GetDefaultField<TEntity>()
    {
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var nameField = properties
            .FirstOrDefault(p => string.Equals(p.Name, "Name", StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (!string.IsNullOrEmpty(nameField))
            return nameField;

        var titleField = properties
            .FirstOrDefault(p => string.Equals(p.Name, "Title", StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (!string.IsNullOrEmpty(titleField))
            return titleField;

        var stringField = properties
            .FirstOrDefault(p => p.PropertyType == typeof(string))
            ?.Name;

        if (!string.IsNullOrEmpty(stringField))
            return stringField;

        return properties.FirstOrDefault()?.Name ?? "Id";
    }
}
