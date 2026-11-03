namespace Whyland.Domain.Model.Commerce.Coupon.Identifiers;

/// <summary>
/// Key of a coupon row: a discount code accepted at checkout (s9.5). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The GUID the row was created with.</param>
public readonly record struct CouponId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>Wraps a key that arrives as a raw guid — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static CouponId From(Guid value) => new(value);
}
