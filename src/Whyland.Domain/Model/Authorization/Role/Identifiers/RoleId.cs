namespace Whyland.Domain.Model.Authorization.Role.Identifiers;

/// <summary>
/// Key of a role row: a named bundle of permissions (s6.1). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The identity number PostgreSQL generates for the row.</param>
public readonly record struct RoleId(long Value) : IStronglyTypedId<long>
{
    /// <summary>Wraps a key that arrives as a raw long — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static RoleId From(long value) => new(value);
}
