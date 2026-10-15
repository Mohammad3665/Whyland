using Microsoft.AspNetCore.Authorization;

namespace Whyland.Infrastructure.Identity.Attributes;

/// <summary>
/// Authorizes users based on specific permission requirements.
/// </summary>
/// <remarks>
/// Usage: [HasPermission(Permissions.Products.Create)] on controllers or actions.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
