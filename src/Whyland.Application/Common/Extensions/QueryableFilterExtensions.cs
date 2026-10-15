using System.Linq.Expressions;
using Whyland.Application.Common.Helpers;

namespace Whyland.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for applying filter conditions to <see cref="IQueryable{T}"/> collections.
/// </summary>
public static class QueryableFilterExtensions
{
    /// <summary>
    /// Applies a collection of filter conditions to the query.
    /// Conditions that cannot be resolved to a property/type are skipped defensively.
    /// </summary>
    /// <typeparam name="T">The type of entities in the queryable collection.</typeparam>
    /// <param name="query">The queryable collection to apply filters to.</param>
    /// <param name="filters">The collection of filter conditions to apply.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with the filter conditions applied.</returns>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, IEnumerable<FilterCondition>? filters)
    {
        if (filters is null) return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Property)) continue;

            var prop = typeof(T).GetProperty(filter.Property,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop is null) continue;

            var member = Expression.Property(parameter, prop);
            var targetType = prop.PropertyType;

            Expression? predicate = null;

            switch (filter.Operator)
            {
                case FilterOperator.Equals:
                case FilterOperator.NotEquals:
                case FilterOperator.GreaterThan:
                case FilterOperator.GreaterThanOrEqual:
                case FilterOperator.LessThan:
                case FilterOperator.LessThanOrEqual:
                {
                    if (!TypeConversionHelper.TryConvert(filter.Value, targetType, out var converted)) continue;

                    var constant = Expression.Constant(converted, targetType);
                    predicate = filter.Operator switch
                    {
                        FilterOperator.Equals => Expression.Equal(member, constant),
                        FilterOperator.NotEquals => Expression.NotEqual(member, constant),
                        FilterOperator.GreaterThan => Expression.GreaterThan(member, constant),
                        FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
                        FilterOperator.LessThan => Expression.LessThan(member, constant),
                        FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
                        _ => null
                    };
                }
                break;

                case FilterOperator.Contains:
                case FilterOperator.StartsWith:
                case FilterOperator.EndsWith:
                {
                    if (prop.PropertyType != typeof(string)) continue;
                    if (filter.Value is null) continue;

                    var methodName = filter.Operator is FilterOperator.Contains ? "Contains"
                        : filter.Operator is FilterOperator.StartsWith ? "StartsWith"
                        : "EndsWith";
                    var method = typeof(string).GetMethod(methodName, [typeof(string)])!;
                    var constant = Expression.Constant(filter.Value, typeof(string));

                    predicate = Expression.Call(member, method, constant);
                }
                break;

                case FilterOperator.In:
                {
                    if (string.IsNullOrEmpty(filter.Value)) continue;
                    var parts = filter.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();
                    if (parts.Length is 0) continue;

                    Expression? orExpr = null;
                    foreach (var part in parts)
                    {
                        if (!TypeConversionHelper.TryConvert(part, targetType, out var conv)) continue;
                        var constant = Expression.Constant(conv, targetType);
                        var eq = Expression.Equal(member, constant);
                        orExpr = orExpr is null ? eq : Expression.OrElse(orExpr, eq);
                    }
                    predicate = orExpr;
                }
                break;
            }

            if (predicate is null) continue;
            combined = combined is null ? predicate : Expression.AndAlso(combined, predicate);
        }

        if (combined is null) return query;
        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }
}
