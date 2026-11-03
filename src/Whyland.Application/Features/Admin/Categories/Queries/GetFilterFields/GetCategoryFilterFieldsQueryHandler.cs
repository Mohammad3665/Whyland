namespace Whyland.Application.Features.Admin.Categories.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetCategoryFilterFieldsQuery"/> by reading the category properties and the
/// two columns the listing computes.
/// A new scalar column shows up in the picker by itself — it only has to be in the query
/// repository's whitelist — so only its Persian label belongs here.
/// </summary>
public sealed class GetCategoryFilterFieldsQueryHandler :
    IQueryHandler<GetCategoryFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
    private static readonly string[] Equality =
    [
        nameof(FilterOperator.Equals), nameof(FilterOperator.NotEquals)
    ];

    private static readonly string[] Text =
    [
        .. Equality,
        nameof(FilterOperator.Contains), nameof(FilterOperator.StartsWith), nameof(FilterOperator.EndsWith)
    ];

    private static readonly string[] Range =
    [
        .. Equality,
        nameof(FilterOperator.GreaterThan), nameof(FilterOperator.GreaterThanOrEqual),
        nameof(FilterOperator.LessThan), nameof(FilterOperator.LessThanOrEqual)
    ];

    private static readonly string[] List = [.. Equality, nameof(FilterOperator.In)];

    /// <summary>
    /// Persian labels for the category fields. Anything not listed keeps its property name,
    /// so a missing label costs a label, never a filter.
    /// </summary>
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه",
        ["Name"] = "نام",
        ["LatinName"] = "نام لاتین",
        ["DisplayOrder"] = "ترتیب نمایش",
        ["ParentId"] = "شناسه والد",
        ["ParentName"] = "نام والد",
        ["Depth"] = "سطح درخت",
        ["IsActive"] = "وضعیت فعال",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["CreatedBy"] = "ایجاد شده توسط",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
        ["UpdatedBy"] = "ویرایش شده توسط",
        ["DeletedAt"] = "تاریخ حذف",
        ["DeletedBy"] = "حذف شده توسط",
        ["IsDeleted"] = "حذف‌شده",
    };

    /// <summary>
    /// The columns the listing computes from the joined parent rows instead of reading them from
    /// the category itself, so reflection over the entity cannot see them. Both are whitelisted by
    /// <c>CategoryQueryRepository</c>, which is what makes them filterable.
    /// </summary>
    private static readonly FilterFieldDto[] Computed =
    [
        new("ParentName", Labels["ParentName"], "string", Text),

        // The level a row can sit on is closed: the entity owns both ends of that range, so the
        // picker refuses a value outside it instead of querying a level that cannot exist.
        new("Depth", Labels["Depth"], "number", Range, null, Category.MinDepth, Category.MaxDepth),
    ];

    /// <summary>
    /// Returns the filterable fields of a category: the entity's own scalar properties in
    /// declaration order, then the computed tree columns.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetCategoryFilterFieldsQuery query, CancellationToken ct)
    {
        var fields = typeof(Domain.Model.Catalog.Category.Entities.Category)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead)
            .Select(Describe)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        fields.AddRange(Computed);

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }

    /// <summary>
    /// Maps one property to its picker entry, or null when it cannot be filtered by a single
    /// value (navigations and collections such as the children or the course links).
    /// </summary>
    /// <param name="property">The category property.</param>
    private static FilterFieldDto? Describe(PropertyInfo property)
    {
        var label = Labels.TryGetValue(property.Name, out var text) ? text : property.Name;

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
            // existing users; any other guid stays a typed value.
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

    /// <summary>Number types matched explicitly, so collection types are never offered.</summary>
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];
}
