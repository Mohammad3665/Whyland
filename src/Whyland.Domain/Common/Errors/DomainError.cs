using Whyland.Shared.Results;

namespace Whyland.Domain.Common.Errors;

/// <summary>
/// An error owned by a name space: an aggregate (<c>Faq.NotFound</c>) or a cross-cutting catalog
/// (<c>Validation.TooLong</c>, <c>System.SmsNotSent</c>).
/// Inherits from <see cref="Error"/> so it flows through <see cref="Results.Result"/> seamlessly.
/// <para>
/// The qualified code is the contract: it is the key the message is looked up with, and the message
/// itself is resolved at the boundary that serves the user, never authored here.
/// </para>
/// </summary>
public sealed record DomainError : Error
{
    private DomainError(
        string code,
        ErrorType type,
        string? field,
        IReadOnlyDictionary<string, object?> arguments)
        // Description stays null on purpose; a non-null description would mean text authored in code.
        : base(Code: code, Type: type)
    {
        Field = field;
        Arguments = arguments;
    }

    /// <summary>
    /// Creates an error belonging to <typeparamref name="TEntity"/>; the code becomes <c>{Entity}.{name}</c>.
    /// </summary>
    /// <param name="name">The error name inside the aggregate (e.g. <c>"FieldRequired"</c>).</param>
    /// <param name="type">The category of the error.</param>
    /// <param name="field">The field the error is bound to, if any.</param>
    /// <param name="arguments">Key/value arguments that fill the placeholders of the resolved message.</param>
    public static DomainError For<TEntity>(
        string name,
        ErrorType type,
        string? field = null,
        params (string Key, object? Value)[] arguments)
        where TEntity : IEntity
        => For(typeof(TEntity).Name, name, type, field, arguments);

    /// <summary>
    /// Creates an error owned by a catalog that is not an aggregate, so its code is <c>{prefix}.{name}</c>.
    /// </summary>
    /// <param name="prefix">The catalog the code is namespaced under (e.g. <c>"Validation"</c>).</param>
    /// <param name="name">The error name inside the catalog.</param>
    /// <param name="type">The category of the error.</param>
    /// <param name="field">The field the error is bound to, if any.</param>
    /// <param name="arguments">Key/value arguments that fill the placeholders of the resolved message.</param>
    public static DomainError For(
        string prefix,
        string name,
        ErrorType type,
        string? field = null,
        params (string Key, object? Value)[] arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var args = arguments.Length == 0
            ? EmptyArguments
            : arguments.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

        return new DomainError($"{prefix}.{name}", type, field, args);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyArguments =
        new Dictionary<string, object?>(0, StringComparer.Ordinal);

    public static implicit operator Result(DomainError error) => Result.Failure(error);
}
