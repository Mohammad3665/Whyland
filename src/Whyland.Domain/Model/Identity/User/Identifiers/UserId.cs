namespace Whyland.Domain.Model.Identity.User.Identifiers;

/// <summary>
/// Key of an account, reached by its phone number (s5.2). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The GUID the row was created with.</param>
public readonly record struct UserId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>Wraps a key that arrives as a raw guid — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static UserId From(Guid value) => new(value);
}
