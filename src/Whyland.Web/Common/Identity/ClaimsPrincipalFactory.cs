using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Whyland.Shared.Dtos.Identity;
using Whyland.Web.Common.Identity.Constants;

namespace Whyland.Web.Common.Identity;

/// <summary>
/// Builds the ClaimsPrincipal for the auth cookie from a successful login result.
/// Each effective permission becomes one "permissions" claim, exactly what
/// PermissionAuthorizationHandler checks (s6.4, s6.5).
/// </summary>
public static class ClaimsPrincipalFactory
{
    /// <summary>Creates an authenticated principal carrying identity + permission claims.</summary>
    /// <param name="user">The login result (id, name, permissions).</param>
    public static ClaimsPrincipal Create(AuthenticatedUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
        };

        // One claim per permission key — read by PermissionAuthorizationHandler.
        claims.AddRange(user.Permissions.Select(p => new Claim(CustomClaimTypes.Permissions, p)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}