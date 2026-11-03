namespace Whyland.Domain.Model.Catalog.CourseSection.Identifiers;

/// <summary>
/// Key of a section that groups the episodes of a course (s7.2.7). Wrapping the column type in its own type keeps an id of one aggregate
/// from being handed to another, and it is the only place that knows how this key is generated.
/// </summary>
/// <param name="Value">The identity number PostgreSQL generates for the row.</param>
public readonly record struct CourseSectionId(long Value) : IStronglyTypedId<long>
{
    /// <summary>Wraps a key that arrives as a raw long — a route value, a database read or a request field.</summary>
    /// <param name="value">The raw key value.</param>
    public static CourseSectionId From(long value) => new(value);
}
