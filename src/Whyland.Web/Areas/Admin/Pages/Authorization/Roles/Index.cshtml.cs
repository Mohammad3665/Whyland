using Whyland.Application.Features.Admin.Authorization.Roles.Commands.Create;
using Whyland.Application.Features.Admin.Authorization.Roles.Commands.Delete;
using Whyland.Application.Features.Admin.Authorization.Roles.Commands.SavePermissions;
using Whyland.Application.Features.Admin.Authorization.Roles.Commands.Update;
using Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetGrantedPermissions;
using Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetPermissionCatalog;
using Whyland.Domain.Model.Authorization.Role.Identifiers;
using Whyland.Shared.Dtos.Authorization.Permission;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Authorization.Roles;

/// <summary>
/// «نقش‌ها» admin page (sections 5.1 – 5.3 of the UI/UX doc): the dynamic Role entities, wired to
/// the role features through <see cref="IDispatcher"/>.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Roles.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>
    /// Persian heading of each module, keyed by the prefix of its permission keys
    /// (<c>role.view</c> → <c>role</c>). A key whose prefix is not listed is still shown — under its
    /// own prefix — because a new module must not silently disappear from the tree.
    /// </summary>
    private static readonly Dictionary<string, string> GroupTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["course"] = "دوره‌ها",
        ["category"] = "دسته‌بندی‌ها",
        ["user"] = "کاربران",
        ["role"] = "نقش‌ها",
        ["permission"] = "دسترسی‌ها",
        ["order"] = "سفارش‌ها",
        ["payment"] = "پرداخت‌ها",
        ["coupon"] = "کدهای تخفیف",
        ["blog"] = "مقالات",
        ["faq"] = "سوالات متداول",
        ["setting"] = "تنظیمات",
        ["dashboard"] = "داشبورد",
    };

    /// <summary>
    /// The permission catalogue grouped by module, in the order the tree is rendered. Empty when the
    /// read failed — the modal then shows an empty tree rather than a page that cannot load.
    /// </summary>
    public IReadOnlyList<PermissionGroup> PermissionGroups { get; private set; } = [];

    /// <summary>
    /// Renders the page scaffold and the permission tree; the table body is filled through
    /// <c>?handler=Rows</c> and the ticked boxes through <c>?handler=Granted</c>.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task OnGetAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetPermissionCatalogQuery(), ct);

        // The tree is the only server-rendered part of this page, and a failed read costs the
        // operator the modal, not the whole list, so an empty catalogue is tolerated.
        var catalog = result.IsSuccess ? result.Data : [];

        // Grouping by the key prefix is what makes the tree match the code: a permission can only be
        // granted to the module that declares it, and a module the seeder added on its own still
        // shows up (under its own prefix) instead of disappearing.
        PermissionGroups = [
            .. catalog
                .GroupBy(permission => ModuleOf(permission.Key))
                .Select(group => new PermissionGroup(
                    group.Key,
                    GroupTitle(group.Key),
                    [.. group])),
        ];
    }

    /// <summary>The module prefix of a permission key: everything before the first dot.</summary>
    /// <param name="key">The permission key, e.g. <c>role.create</c>.</param>
    public static string ModuleOf(string key)
    {
        var dot = key.IndexOf('.');
        return dot < 0 ? key : key[..dot];
    }

    /// <summary>The Persian heading a permission key is grouped under.</summary>
    /// <param name="key">The permission key, e.g. <c>role.create</c>.</param>
    public static string GroupTitle(string key)
    {
        var module = ModuleOf(key);
        return GroupTitles.TryGetValue(module, out var title) ? title : module;
    }

    /// <summary>
    /// Returns one page of roles for the server-side DataTable, with the sort column, the searched
    /// field and any number of generic field/operator/value conditions. With no column chosen the
    /// listing keeps its own order (title), so this page adds no default of its own.
    /// </summary>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="filters">Conditions built by the filter picker; ANDed together.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The flat <c>DataTableResult&lt;RoleListItemDto&gt;</c> envelope, or a 500 with a message.</returns>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter, List<FilterCondition>? filters, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetRolesForDataTableQuery(filter, filters), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the fields the table can be filtered or sorted on, with the operators each of them
    /// accepts — including the two counted columns, which are computed rather than stored.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFieldsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetRoleFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the permission identifiers the role already holds, so the modal can tick them before
    /// the operator touches the tree.
    /// </summary>
    /// <param name="id">The role whose grants are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetGrantedAsync([FromQuery] RoleId id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetRoleGrantedPermissionsQuery(id), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    #region Mutations

    // Each write demands its own key on top of the view key the class carries: opening the list must
    // not be enough to change it. The check is made in code because an attribute on a page handler
    // method is ignored by Razor Pages (MVC1001).

    /// <summary>Creates a custom role (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateRoleCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Roles.Create)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Renames a custom role and retitles it (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the role id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateRoleCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Roles.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>
    /// Soft-deletes a custom role (?handler=Delete). The command also detaches it from every account
    /// and revokes its grants, in the same save.
    /// </summary>
    /// <param name="id">The role to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] RoleId id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Roles.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteRoleCommand(id), ct));

    /// <summary>
    /// Rewrites a custom role's permission set (?handler=SavePermissions) with every identifier the
    /// tree has ticked; an empty set revokes everything.
    /// </summary>
    /// <param name="command">The role id and the checked permission identifiers.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostSavePermissionsAsync(
        SaveRolePermissionsCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Roles.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    #endregion

    /// <summary>
    /// One heading of the permission tree: the module prefix of the keys it holds, its Persian
    /// title and the permissions themselves.
    /// </summary>
    /// <param name="Module">The key prefix, e.g. <c>role</c>; used as the group id in the markup.</param>
    /// <param name="Title">The heading shown above the group.</param>
    /// <param name="Items">The permissions of that module, in catalogue order.</param>
    public sealed record PermissionGroup(
        string Module,
        string Title,
        IReadOnlyList<PermissionOptionDto> Items);
}
