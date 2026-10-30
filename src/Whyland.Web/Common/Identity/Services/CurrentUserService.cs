namespace Whyland.Web.Common.Identity.Services;

/// <summary>
/// Reads the authenticated user off the current HTTP context (the cookie principal).
/// Lives in the Web layer because it needs HttpContext; overrides the Infrastructure
/// NullCurrentUserService via DI last-registration-wins.
/// </summary>
/// <param name="httpContextAccessor">Accessor for the current request's context.</summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    /// <summary>The UserId (NameIdentifier claim) of the signed-in user, or null.</summary>
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>True only when the principal is authenticated AND carries a UserId.</summary>
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true && UserId.HasValue;
}