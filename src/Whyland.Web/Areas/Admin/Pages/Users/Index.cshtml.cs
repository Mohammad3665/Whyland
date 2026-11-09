using Whyland.Application.Features.Admin.Users.Commands.Create;
using Whyland.Application.Features.Admin.Users.Commands.ToggleStatus;
using Whyland.Application.Features.Admin.Users.Commands.Update;
using Whyland.Application.Features.Admin.Users.Queries.GetDetails;
using Whyland.Application.Features.Admin.Users.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.Users.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Users.Queries.GetOptions;
using Whyland.Application.Features.Admin.Users.Queries.GetRoles;
using Whyland.Domain.Model.Identity.User.Identifiers;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Users;

/// <summary>
/// «کاربران» admin page (sections 4.1 / 4.2 of the UI/UX doc): the authentication accounts,
/// wired to the admin users feature through <see cref="IDispatcher"/>.
/// <para>
/// The table runs server-side: paging, sorting and every filter condition are resolved by the
/// generic <see cref="QueryRequest"/> pipeline, so no field has its own query and adding a
/// column does not touch this page.
/// </para>
/// Rows come from <c>?handler=Rows</c>, the filter picker from <c>?handler=Fields</c>, the
/// details modal from <c>?handler=Details</c>; mutations run through their command handlers.
/// Accounts are never deleted — <c>IsActive</c> is the supported state change.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Users.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>Roles offered in the create/edit modal; empty when the read failed.</summary>
    public IReadOnlyList<RoleOptionDto> Roles { get; private set; } = [];

    /// <summary>Renders the page scaffold; the table body is filled through <c>?handler=Rows</c>.</summary>
    public async Task OnGetAsync(CancellationToken ct)
        => Roles = await GetRolesAsync(ct);

    /// <summary>
    /// Returns one page of accounts for the server-side DataTable, with the sort column,
    /// the searched field and any number of generic field/operator/value conditions.
    /// </summary>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="filters">Conditions built by the filter picker; ANDed together.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The flat <c>DataTableResult&lt;UserListItemDto&gt;</c> envelope, or a 500 with a message.</returns>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter, List<FilterCondition>? filters, CancellationToken ct)
    {
        // Newest account first when the table has not picked a column yet.
        if (string.IsNullOrWhiteSpace(filter.SortColumn))
        {
            filter.SortColumn = "createdAt";
            filter.SortColumnDirection = "desc";
        }

        var result = await dispatcher.Send(
            new GetUsersForDataTableQuery(filter, filters), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the fields the table can be filtered or sorted on, with the operators each of
    /// them accepts, so the picker is built from the model instead of from markup.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFieldsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetUserFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the accounts that hold the permission behind the requested author field, so the
    /// «created by» / «last edited by» filters list the people who could have done it — not every
    /// account — and are set by picking a name instead of pasting a guid.
    /// </summary>
    /// <param name="field">The filter field being filled, e.g. <c>CreatedBy</c>.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetUserOptionsAsync([FromQuery] string field, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetUserOptionsQuery(field), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the roles the account list can be filtered by, so the «نقش» field is answered by
    /// picking one of them instead of typing the id of a row the admin cannot see.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetRoleOptionsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetAssignableRolesQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the full account for the details modal: roles, password state, profile data,
    /// provider links and the login-security counters.
    /// </summary>
    /// <param name="id">The account to read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetDetailsAsync([FromQuery] UserId id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetUserDetailsQuery(id), ct);

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
    /// <summary>Creates an account from the create modal (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateUserCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Users.Create)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an account from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the user id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateUserCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Users.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>
    /// Flips an account between active and inactive (?handler=ToggleStatus) and returns the new
    /// state in <c>data</c> so the row badge can be re-rendered without reloading the table.
    /// </summary>
    /// <param name="id">The account to change.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] UserId id, CancellationToken ct)
    {
        if (await DenyUnlessAsync(Permissions.Users.Edit) is { } denied)
            return denied;

        var result = await dispatcher.Send(new ToggleUserStatusCommand(id), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    #endregion

    /// <summary>Reads the role list, tolerating a failure with an empty select.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    private async Task<IReadOnlyList<RoleOptionDto>> GetRolesAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetAssignableRolesQuery(), ct);
        return result.IsSuccess ? result.Data : [];
    }
}
