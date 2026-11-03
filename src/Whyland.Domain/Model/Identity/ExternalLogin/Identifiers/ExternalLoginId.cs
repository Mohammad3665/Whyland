namespace Whyland.Domain.Model.Identity.ExternalLogin.Identifiers;

/// <summary>
/// Key of a link between an account and an external identity provider (s5.4). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The GUID the row was created with.</param>
public readonly record struct ExternalLoginId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>Wraps a key that arrives as a raw guid — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static ExternalLoginId From(Guid value) => new(value);
}
