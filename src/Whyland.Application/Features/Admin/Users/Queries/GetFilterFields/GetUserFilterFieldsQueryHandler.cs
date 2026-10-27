using System.Reflection;
using Whyland.Shared.Enums;
using Whyland.Shared.Filters;

namespace Whyland.Application.Features.Admin.Users.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetUserFilterFieldsQuery"/> by reading the account properties.
/// A new scalar field on the entity shows up in the picker by itself; only its Persian
/// label has to be added to <see cref="Labels"/>.
/// </summary>
public sealed class GetUserFilterFieldsQueryHandler :
    IQueryHandler<GetUserFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
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
    /// Persian labels for the account fields. Anything not listed keeps its property name,
    /// so a missing label costs a label, never a filter.
    /// </summary>
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه",
        ["FirstName"] = "نام",
        ["LastName"] = "نام خانوادگی",
        ["Email"] = "ایمیل",
        ["PhoneNumber"] = "شماره موبایل",
        ["IsActive"] = "وضعیت حساب",
        ["LastLoginAt"] = "آخرین ورود",
        ["FailedLoginAttempts"] = "تلاش‌های ناموفق ورود",
        ["LockoutUntilUtc"] = "قفل حساب تا",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["CreatedBy"] = "ایجاد شده توسط",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
        ["UpdatedBy"] = "ویرایش شده توسط",
        ["DeletedBy"] = "حذف شده توسط",
        ["IsDeleted"] = "حذف‌شده",
    };

    /// <summary>Secret material is never offered as a filter field.</summary>
    private static readonly HashSet<string> Hidden = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Domain.Entities.Identity.User.PasswordHash),
    };

    /// <summary>
    /// Returns the filterable fields of the account, in declaration order.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetUserFilterFieldsQuery query, CancellationToken ct)
    {
        var fields = typeof(Domain.Entities.Identity.User)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && !Hidden.Contains(x.Name))
            .Select(Describe)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }

    /// <summary>
    /// Maps one property to its picker entry, or null when it cannot be filtered by a single
    /// value (navigations and collections such as the assigned roles).
    /// </summary>
    /// <param name="property">The account property.</param>
    private static FilterFieldDto? Describe(PropertyInfo property)
    {
        var label = Labels.TryGetValue(property.Name, out var text) ? text : property.Name;
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

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
}
