using Whyland.Shared.Errors;
using Whyland.Shared.Results;

namespace Whyland.Web.Areas.Admin.Pages;

/// <summary>
/// Base for admin page models whose individual handlers need their own permission key.
/// </summary>
/// <remarks>
/// Razor Pages reads authorization attributes from the page model class only, so a page carries one
/// policy for all of its handlers and an attribute on a handler method is reported as ignored
/// (MVC1001). A page is therefore gated by its "view" key at the class and each write asks the
/// authorization service here. This is the single place that turns a failed check into the
/// <c>{isSuccess, message}</c> envelope the admin tables already read.
/// </remarks>
/// <param name="authorizationService">Resolves the dynamic policy of a permission key.</param>
public abstract class AdminPageModel(IAuthorizationService authorizationService) : PageModel
{
    /// <summary>
    /// Checks one permission key against the signed-in account.
    /// </summary>
    /// <param name="permission">The key the handler requires, e.g. <c>Permissions.Categories.Delete</c>.</param>
    /// <returns><c>null</c> when the account holds the key; otherwise the response the handler must return.</returns>
    protected async Task<IActionResult?> DenyUnlessAsync(string permission)
    {
        // The permission key doubles as the policy name, which is what [HasPermission] does too:
        // PermissionPolicyProvider builds the requirement for any key it has no named policy for.
        var result = await authorizationService.AuthorizeAsync(User, null, permission);

        if (result.Succeeded)
            return null;

        // A denial is reported like any other rejected write so a modal shows its toast instead of
        // replacing the page with an HTML error in the middle of an action.
        return new JsonResult(
            new
            {
                isSuccess = false,
                message = "اجازهٔ انجام این عملیات را ندارید.",
            })
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
    }

    /// <summary>
    /// Flattens any <see cref="BaseResult"/> (with or without data) into the JSON envelope the admin
    /// pages consume: 200 with <c>isSuccess=true</c> on success, 400 with the server-side error message
    /// (validation, not-found, ...) otherwise.
    /// <para>
    /// Every page already reads this shape, so it belongs to the base rather than to each page: three
    /// private copies meant three chances for the same envelope to drift.
    /// </para>
    /// </summary>
    /// <param name="result">The dispatched command outcome.</param>
    protected static JsonResult ToJson(BaseResult result)
        => result.IsSuccess
            ? new JsonResult(new { isSuccess = true, message = (string?)null })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };

    /// <summary>
    /// Builds a readable message out of an <see cref="Error"/>: aggregated validation failures carry
    /// the individual errors in <c>Arguments["Errors"]</c>, everything else uses the plain description.
    /// </summary>
    /// <param name="error">The error to describe.</param>
    protected static string MainMessage(Error error)
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
