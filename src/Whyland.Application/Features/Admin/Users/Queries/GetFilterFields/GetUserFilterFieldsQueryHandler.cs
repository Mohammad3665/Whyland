using Whyland.Application.Common.Helpers;

namespace Whyland.Application.Features.Admin.Users.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetUserFilterFieldsQuery"/> by reading the account properties.
/// A new scalar field on the entity shows up in the picker by itself; only its Persian
/// label has to be added to <see cref="Labels"/>. The property-to-entry mapping itself lives
/// in <see cref="FilterFieldCatalog"/>, shared with every other admin table.
/// </summary>
public sealed class GetUserFilterFieldsQueryHandler :
    IQueryHandler<GetUserFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
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
    };

    /// <summary>Secret material is never offered as a filter field.</summary>
    private static readonly string[] Hidden =
    [
        nameof(Domain.Model.Identity.User.Entities.User.PasswordHash),
    ];

    /// <summary>
    /// Returns the filterable fields of the account, in declaration order, followed by the
    /// relationships the list can be narrowed by.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetUserFilterFieldsQuery query, CancellationToken ct)
    {
        var fields = new List<FilterFieldDto>(
            FilterFieldCatalog.Describe(typeof(Domain.Model.Identity.User.Entities.User), Labels, Hidden));

        // A role is not a property of the account: it is a row of the assignment table, so
        // reflection cannot offer it and this page names it. "role" is the kind the picker dresses
        // as a list of roles and the engine answers with an EXISTS over that table.
        fields.Add(new FilterFieldDto("Role", "نقش", "role", FilterFieldCatalog.Equality));

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }
}
