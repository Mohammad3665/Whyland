using Whyland.Domain.Common.Primitives;
using Whyland.Shared.Errors;

namespace Whyland.Domain.Common.Errors;

/// <summary>
/// A domain-level error tied to a specific entity type.
/// Inherits from <see cref="Error"/> so it flows through <see cref="Results.Result"/> seamlessly.
/// </summary>
public sealed record DomainError : Error
{
    private DomainError(
        Type entityType,
        string name,
        ErrorType type,
        string? field,
        string? description,
        IReadOnlyDictionary<string, object?> arguments)
        : base(
            Code: $"{entityType.Name}.{name}",
            Description: description ?? name,
            Type: type)
    {
        EntityType = entityType;
        Name = name;
        Field = field;
        Arguments = arguments;
    }

    /// <summary>The CLR type of the entity this error belongs to.</summary>
    public Type EntityType { get; }

    /// <summary>The error name within the entity (e.g. <c>"FieldRequired"</c>).</summary>
    public string Name { get; }

    /// <summary>
    /// Creates a <see cref="DomainError"/> bound to <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="code">The error name (e.g. <c>"FieldRequired"</c>).</param>
    /// <param name="type">The category of the error.</param>
    /// <param name="field">The field the error is bound to, if any.</param>
    /// <param name="description">Optional human-readable description. Defaults to <paramref name="code"/>.</param>
    /// <param name="arguments">Key/value arguments for templating.</param>
    public static DomainError For<TEntity>(
        string code,
        ErrorType type,
        string? field = null,
        string? description = null,
        params (string Key, object? Value)[] arguments)
        where TEntity : IEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var args = arguments.Length == 0
            ? EmptyArguments
            : arguments.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

        return new DomainError(typeof(TEntity), code, type, field, description, args);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyArguments =
        new Dictionary<string, object?>(0, StringComparer.Ordinal);
}