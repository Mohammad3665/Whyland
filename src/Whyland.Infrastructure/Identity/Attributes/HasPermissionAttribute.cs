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
    /// <summary>
    /// Creates the attribute and authorizes with the dynamic policy matching
    /// the given permission key (s6.5).
    /// </summary>
    /// <param name="permission">The permission key required to access the decorated member.</param>
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
