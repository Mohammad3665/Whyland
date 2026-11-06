namespace Whyland.Application.Common.Helpers;

/// <summary>
/// The single definition of "which fields can a list page be filtered on, and how".
/// <para>
/// Every admin table builds its filter picker from the entity that backs it, so an operator can
/// filter on any column the model has without a page writing its own field list. That makes the
/// mapping from a CLR property to a picker entry logic that must exist exactly once: a per-page copy
/// would let the users picker and the roles picker drift apart in what the same operator name means.
/// </para>
/// <para>
/// A page contributes only what is genuinely its own — the Persian labels and the fields it refuses
/// to expose — and gets the rest from here.
/// </para>
/// </summary>
public static class FilterFieldCatalog
{
    #region Operator sets

    /// <summary>Operators every field type accepts.</summary>
    public static readonly IReadOnlyList<string> Equality =
    [
        nameof(FilterOperator.Equals), nameof(FilterOperator.NotEquals)
    ];

    /// <summary>Operators for a text column: equality plus the substring shapes.</summary>
    public static readonly IReadOnlyList<string> Text =
    [
        .. Equality,
        nameof(FilterOperator.Contains), nameof(FilterOperator.StartsWith), nameof(FilterOperator.EndsWith)
    ];

    /// <summary>Operators for an ordered column: equality plus the comparisons.</summary>
    public static readonly IReadOnlyList<string> Range =
    [
        .. Equality,
        nameof(FilterOperator.GreaterThan), nameof(FilterOperator.GreaterThanOrEqual),
        nameof(FilterOperator.LessThan), nameof(FilterOperator.LessThanOrEqual)
    ];

    /// <summary>Operators for a closed domain: equality plus "one of these".</summary>
    public static readonly IReadOnlyList<string> List = [.. Equality, nameof(FilterOperator.In)];

    #endregion

    #region Discovery

    /// <summary>
    /// Describes every readable property of the entity that a single value can filter on, in
    /// declaration order. Navigations and collections are skipped, not failed: a page that offers
    /// them would send a condition the shared engine cannot resolve.
    /// </summary>
    /// <param name="entityType">The entity whose properties build the picker.</param>
    /// <param name="labels">Persian labels; anything missing keeps its property name.</param>
    /// <param name="hidden">Properties that must never be offered (secrets, soft-delete bookkeeping).</param>
    public static IReadOnlyList<FilterFieldDto> Describe(
        Type entityType,
        IReadOnlyDictionary<string, string> labels,
        IReadOnlyCollection<string> hidden)
    {
        var hiddenNames = new HashSet<string>(hidden, StringComparer.OrdinalIgnoreCase);

        return entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && !hiddenNames.Contains(x.Name))
            .Select(x => Describe(x, labels))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    /// <summary>
    /// Maps one property to its picker entry, or null when it cannot be filtered by a single
    /// value (navigations and collections such as the assigned roles).
    /// </summary>
    /// <param name="property">The entity property.</param>
    /// <param name="labels">Persian labels; anything missing keeps its property name.</param>
    public static FilterFieldDto? Describe(PropertyInfo property, IReadOnlyDictionary<string, string> labels)
    {
        var label = labels.TryGetValue(property.Name, out var text) ? text : property.Name;

        // The picker offers the type the column stores, so an identifier is opened into the key it
        // wraps; left wrapped it would look like an unknown struct and the field would vanish from
        // the picker even though the query repository whitelists it.
        var type = StronglyTypedIds.Unwrap(property.PropertyType);

        if (type == typeof(string))
            return new FilterFieldDto(property.Name, label, "string", Text);

        if (type == typeof(bool))
            return new FilterFieldDto(property.Name, label, "bool", Equality, ["true", "false"]);

        if (type == typeof(Guid))
            // The audit columns name an account, so they are filtered by picking one of the
            // existing users; any other guid (the id of the row itself) stays a typed value.
            return property.Name.EndsWith("By", StringComparison.Ordinal)
                ? new FilterFieldDto(property.Name, label, "user", List)
                : new FilterFieldDto(property.Name, label, "guid", List);

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
            return new FilterFieldDto(property.Name, label, "date", Range);

        if (type.IsEnum)
            return new FilterFieldDto(property.Name, label, "enum", List, [.. Enum.GetNames(type)]);

        if (NumericTypes.Contains(type))
            return new FilterFieldDto(property.Name, label, "number", Range);

        // Anything left is a navigation or a collection, which no single value can filter.
        return null;
    }

    /// <summary>Number types matched explicitly, so collection converters are never offered.</summary>
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];

    #endregion
}
