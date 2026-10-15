using System.Linq.Expressions;

namespace Whyland.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for applying sorting to <see cref="IQueryable{T}"/> collections.
/// Sorting is applied dynamically based on property names and sort directions using reflection.
/// Compatible with Entity Framework and other LINQ providers supporting expression-based sorting.
/// </summary>
public static class QueryableSortingExtensions
{
    /// <summary>
    /// Applies sorting to the query based on the specified property name and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of entities in the queryable collection.</typeparam>
    /// <param name="query">The queryable collection to apply sorting to.</param>
    /// <param name="sortBy">The name of the property to sort by; null/whitespace leaves the query unchanged.</param>
    /// <param name="sort">The direction to sort by. Defaults to ascending.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with the sorting applied, or the original query if
    /// the property cannot be resolved.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, SortType sort)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var prop = typeof(T).GetProperty(sortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return query;

        var param = Expression.Parameter(typeof(T), "x");
        var propAccess = Expression.Property(param, prop);
        var lambda = Expression.Lambda(propAccess, param);

        var methodName = sort is SortType.Desc ? "OrderByDescending" : "OrderBy";

        var method = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length is 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
    }
}
