using Whyland.Web.Common.Identity.Providers;

namespace Whyland.Infrastructure.Identity.Handlers;

/// <summary>
/// Handles authorization based on user permissions.
/// </summary>
/// <remarks>
/// Checks if the current user has the required permission claim.
/// </remarks>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// Evaluates whether the user carries the permission claim required by the requirement.
    /// </summary>
    /// <param name="context">The current authorization context.</param>
    /// <param name="requirement">The permission requirement being evaluated.</param>
    /// <returns>A completed task once the requirement has been evaluated.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var permissions = context.User.FindAll(CustomClaimTypes.Permissions).Select(c => c.Value);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
