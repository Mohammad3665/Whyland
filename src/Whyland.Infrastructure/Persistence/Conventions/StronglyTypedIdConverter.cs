using System.Linq.Expressions;

namespace Whyland.Infrastructure.Persistence.Conventions;

/// <summary>
/// A value converter that translates strongly-typed IDs to their underlying primitive value
/// (and vice versa) so EF Core can persist and materialize them correctly.
/// </summary>
/// <typeparam name="TId">The strongly-typed ID type (must be a struct implementing <see cref="IStronglyTypedId{TValue}"/>).</typeparam>
/// <typeparam name="TValue">The underlying primitive type (e.g. int, Guid, long).</typeparam>
/// <remarks>
/// Expression trees are used instead of plain delegates because EF Core needs expressions
/// to translate the conversion into SQL. A simple <c>Func</c> would not be translatable.
///
/// Requirement: <typeparamref name="TId"/> must expose a public constructor that accepts
/// a single <typeparamref name="TValue"/> argument.
/// </remarks>
public sealed class StronglyTypedIdConverter<TId, TValue>() 
    : ValueConverter<TId, TValue>(CreateToProviderExpression(), CreateFromProviderExpression())
    where TId : struct, IStronglyTypedId<TValue>
{
    /// <summary>
    /// Builds the expression used to convert <typeparamref name="TId"/> to <typeparamref name="TValue"/>
    /// when writing to the database. Equivalent to: <c>id => id.Value</c>.
    /// </summary>
    private static Expression<Func<TId, TValue>> CreateToProviderExpression()
        => id => id.Value;

    /// <summary>
    /// Builds the expression used to convert <typeparamref name="TValue"/> back to <typeparamref name="TId"/>
    /// when reading from the database.
    /// Reflection is used to locate the constructor, because <typeparamref name="TId"/> is a generic
    /// type parameter and cannot be instantiated with <c>new TId(value)</c> directly.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="TId"/> does not declare a constructor accepting <typeparamref name="TValue"/>.
    /// </exception>
    private static Expression<Func<TValue, TId>> CreateFromProviderExpression()
    {
        // Locate the constructor that accepts the underlying value type via reflection.
        var constructor = typeof(TId).GetConstructor([typeof(TValue)])
                          ?? throw new InvalidOperationException(
                              $"Type '{typeof(TId).Name}' must have a constructor accepting '{typeof(TValue).Name}'.");

        // Build the expression body: new TId(value)
        var value = Expression.Parameter(typeof(TValue), "value");
        var body = Expression.New(constructor, value);

        return Expression.Lambda<Func<TValue, TId>>(body, value);
    }
}