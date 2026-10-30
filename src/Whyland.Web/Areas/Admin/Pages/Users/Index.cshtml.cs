using Whyland.Application.Features.Admin.Users.Commands.Create;
using Whyland.Application.Features.Admin.Users.Commands.ToggleStatus;
using Whyland.Application.Features.Admin.Users.Commands.Update;
using Whyland.Application.Features.Admin.Users.Queries.GetDetails;
using Whyland.Application.Features.Admin.Users.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.Users.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Users.Queries.GetOptions;
using Whyland.Application.Features.Admin.Users.Queries.GetRoles;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Errors;
using Whyland.Shared.Filters;
using Whyland.Shared.Results;

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
public class IndexModel(IDispatcher dispatcher) : PageModel
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
    /// Returns the full account for the details modal: roles, password state, profile data,
    /// provider links and the login-security counters.
    /// </summary>
    /// <param name="id">The account to read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetDetailsAsync([FromQuery] Guid id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetUserDetailsQuery(id), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    /// <summary>Creates an account from the create modal (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateUserCommand command, CancellationToken ct)
        => ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an account from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the user id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateUserCommand command, CancellationToken ct)
        => ToJson(await dispatcher.Send(command, ct));

    /// <summary>
    /// Flips an account between active and inactive (?handler=ToggleStatus) and returns the new
    /// state in <c>data</c> so the row badge can be re-rendered without reloading the table.
    /// </summary>
    /// <param name="id">The account to change.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnPostToggleStatusAsync([FromForm] Guid id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new ToggleUserStatusCommand(id), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    /// <summary>Reads the role list, tolerating a failure with an empty select.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    private async Task<IReadOnlyList<RoleOptionDto>> GetRolesAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetAssignableRolesQuery(), ct);
        return result.IsSuccess ? result.Data : [];
    }

    /// <summary>
    /// Flattens any <see cref="BaseResult"/> into the JSON envelope the admin pages consume:
    /// 200 with <c>isSuccess=true</c> on success, 400 with the server-side message otherwise.
    /// </summary>
    /// <param name="result">The dispatched command outcome.</param>
    private static JsonResult ToJson(BaseResult result)
        => result.IsSuccess
            ? new JsonResult(new { isSuccess = true, message = (string?)null })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };

    /// <summary>
    /// Builds a readable message out of an <see cref="Error"/>: aggregated validation failures
    /// carry the individual errors in <c>Arguments["Errors"]</c>, everything else uses the
    /// plain description.
    /// </summary>
    /// <param name="error">The error to describe.</param>
    private static string MainMessage(Error error)
    {
        if (error.Arguments?.TryGetValue("Errors", out var value) == true &&
            value is IEnumerable<Error> items)
        {
            var messages = items.Select(e => e.Description);
            return string.Join(' ', messages);
        }

        return error.Description ?? "خطای نامشخص.";
    }
}
