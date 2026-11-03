namespace Whyland.Domain.Model.Catalog.Episode.Identifiers;

/// <summary>
/// Key of an episode row: one playable video of a course section (s7.2.8). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The identity number PostgreSQL generates for the row.</param>
public readonly record struct EpisodeId(long Value) : IStronglyTypedId<long>
{
    /// <summary>Wraps a key that arrives as a raw long — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static EpisodeId From(long value) => new(value);
}
