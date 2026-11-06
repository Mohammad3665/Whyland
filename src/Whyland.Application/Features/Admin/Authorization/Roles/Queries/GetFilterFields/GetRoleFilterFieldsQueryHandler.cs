using Whyland.Application.Common.Helpers;
using Whyland.Shared.Dtos.Authorization.Role;

namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetRoleFilterFieldsQuery"/> by reading the role properties through the shared
/// <see cref="FilterFieldCatalog"/>. A new scalar column on the entity shows up in the picker by
/// itself; only its Persian label has to be added to <see cref="Labels"/>.
/// <para>
/// The counted columns are not entity properties — the listing computes them — so they are described
/// by hand, with the same names the query repository whitelists. A picker entry that no whitelisted
/// column answers to would look usable and do nothing.
/// </para>
/// </summary>
public sealed class GetRoleFilterFieldsQueryHandler :
    IQueryHandler<GetRoleFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
    /// <summary>
    /// Persian labels for the role fields. Anything not listed keeps its property name, so a missing
    /// label costs a label, never a filter.
    /// </summary>
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه",
        ["Name"] = "نام نقش",
        ["Title"] = "عنوان نقش",
        ["IsSystem"] = "نقش سیستمی",
        ["UserCount"] = "تعداد کاربران",
        ["PermissionCount"] = "تعداد دسترسی‌ها",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["CreatedBy"] = "ایجاد شده توسط",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
        ["UpdatedBy"] = "ویرایش شده توسط",
    };

    /// <summary>
    /// Soft-delete bookkeeping is not offered: deleted roles never appear in this listing, so a
    /// condition on those columns could only ever return nothing.
    /// </summary>
    private static readonly string[] Hidden =
    [
        nameof(Role.IsDeleted),
        nameof(Role.DeletedAt),
        nameof(Role.DeletedBy),
    ];

    /// <summary>
    /// Returns the filterable fields of a role: the entity's own columns in declaration order, then
    /// the two counts.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetRoleFilterFieldsQuery query, CancellationToken ct)
    {
        var fields = FilterFieldCatalog.Describe(typeof(Role), Labels, Hidden).ToList();

        fields.Add(new FilterFieldDto(nameof(RoleListItemDto.UserCount), Labels["UserCount"], "number", FilterFieldCatalog.Range));
        fields.Add(new FilterFieldDto(nameof(RoleListItemDto.PermissionCount), Labels["PermissionCount"], "number", FilterFieldCatalog.Range));

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }
}
