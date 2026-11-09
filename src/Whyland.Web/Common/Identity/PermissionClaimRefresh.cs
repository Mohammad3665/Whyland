using Whyland.Application.Common.Abstractions.UnitOfWork;
using Whyland.Domain.Model.Identity.User.Identifiers;

namespace Whyland.Web.Common.Identity;

/// <summary>
/// Keeps the permission claims of a signed-in cookie in step with the database.
/// </summary>
/// <remarks>
/// The auth cookie is the only place the effective permission keys live during a session, so a
/// cookie written before a key existed — or before a module was ever seeded — carries claims that
/// are simply wrong. Without this, every permission change (and the first seed of a new key) would
/// bounce signed-in accounts to the sign-in page until they happened to log in again.
/// <para>
/// The read is throttled by the <see cref="CustomClaimTypes.PermissionsCheckedAt"/> claim instead
/// of running on every request: a session pays for it at most once per <see cref="MaxAge"/>.
/// </para>
/// </remarks>
public static class PermissionClaimRefresh
{
    #region Settings

    /// <summary>How long the permission claims of a cookie may be reused before they are read again.</summary>
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(1);

    #endregion

    #region Handler

    /// <summary>
    /// Cookie validation hook: replaces the permission claims of a live session when they have gone
    /// stale, and renews the cookie so the browser carries the new set.
    /// </summary>
    /// <param name="context">The validation context of the incoming principal.</param>
    public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        // Only the cookie principal issued by ClaimsPrincipalFactory is managed here; anything else
        // (a scheme without a guid subject) is left exactly as it arrived.
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return;

        if (!Guid.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var rawUserId))
            return;

        var userId = UserId.From(rawUserId);

        if (!IsStale(identity, DateTimeOffset.UtcNow))
            return;

        IReadOnlyList<string> keys;
        try
        {
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();

            keys = await unitOfWork.UserQueryRepository.GetPermissionKeysAsync(
                userId, context.HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            // A failed read must not end a session: the claims already in the cookie stay in force
            // and the next request retries after the throttle window.
            context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(PermissionClaimRefresh))
                .LogWarning(ex,
                    "Permission claims of user {UserId} could not be refreshed.", userId);

            return;
        }

        ReplacePermissionClaims(identity, keys, DateTimeOffset.UtcNow);

        // The renewed cookie also carries the new timestamp, which is what keeps the read to once
        // per window instead of once per request.
        context.ShouldRenew = true;
    }

    #endregion

    #region Claim math

    /// <summary>True when the stored check moment is missing or older than the allowed age.</summary>
    /// <param name="identity">The identity of the incoming principal.</param>
    /// <param name="now">The moment compared against the stored timestamp.</param>
    private static bool IsStale(ClaimsIdentity identity, DateTimeOffset now)
    {
        var raw = identity.FindFirst(CustomClaimTypes.PermissionsCheckedAt)?.Value;

        // A cookie from before this claim existed has no timestamp at all: it is stale by definition,
        // which is exactly how an account signed in before a key was seeded gets the new keys.
        if (!long.TryParse(raw, out var seconds))
            return true;

        return DateTimeOffset.FromUnixTimeSeconds(seconds) <= now - MaxAge;
    }

    /// <summary>
    /// Swaps the permission claims of the identity for the freshly read ones and stamps the moment.
    /// </summary>
    /// <param name="identity">The identity being rewritten.</param>
    /// <param name="keys">The effective permission keys of the account right now.</param>
    /// <param name="checkedAt">The moment the keys were read.</param>
    private static void ReplacePermissionClaims(
        ClaimsIdentity identity,
        IReadOnlyList<string> keys,
        DateTimeOffset checkedAt
    )
    {
        RemoveAll(identity, CustomClaimTypes.Permissions);

        foreach (var key in keys)
            identity.AddClaim(new Claim(CustomClaimTypes.Permissions, key));

        RemoveAll(identity, CustomClaimTypes.PermissionsCheckedAt);
        identity.AddClaim(
            new Claim(CustomClaimTypes.PermissionsCheckedAt, checkedAt.ToUnixTimeSeconds().ToString()));
    }

    /// <summary>Removes every claim of one type from the identity.</summary>
    /// <param name="identity">The identity being edited.</param>
    /// <param name="type">The claim type whose claims are dropped.</param>
    private static void RemoveAll(ClaimsIdentity identity, string type)
    {
        // Materialised first: the identity cannot be edited while its claims are still being read.
        foreach (var claim in identity.FindAll(type).ToList())
            identity.RemoveClaim(claim);
    }

    #endregion
}
