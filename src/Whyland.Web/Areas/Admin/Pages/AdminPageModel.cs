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
}
