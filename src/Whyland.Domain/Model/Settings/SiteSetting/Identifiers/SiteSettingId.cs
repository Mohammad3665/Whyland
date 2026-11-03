namespace Whyland.Domain.Model.Settings.SiteSetting.Identifiers;

/// <summary>
/// Key of a site setting row edited from the admin panel (s11.2). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The identity number PostgreSQL generates for the row.</param>
public readonly record struct SiteSettingId(long Value) : IStronglyTypedId<long>
{
    /// <summary>Wraps a key that arrives as a raw long — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static SiteSettingId From(long value) => new(value);
}
