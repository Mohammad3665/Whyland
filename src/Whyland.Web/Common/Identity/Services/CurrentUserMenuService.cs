using Whyland.Application.Common.Abstractions.UnitOfWork;

namespace Whyland.Web.Common.Identity.Services;

/// <summary>
/// Reads what both headers show about the signed-in account — name, phone and avatar come from the
/// database, the panel entry comes from the permission claims of the cookie. One read, one place:
/// the public layout and the admin header ask the same question and must never answer it differently.
/// </summary>
/// <remarks>
/// Returns null for an anonymous visitor and for a cookie whose account is no longer readable, so a
/// header can fall back to the sign-in button instead of rendering an empty menu.
/// </remarks>
/// <param name="unitOfWork">Provides the account read.</param>
public sealed class CurrentUserMenuService(IUnitOfWork unitOfWork)
{
    /// <summary>Builds the menu of the given principal, or null when there is nothing to show.</summary>
    /// <param name="user">The principal of the current request.</param>
    /// <param name="ct">A token to cancel the read.</param>
    public async Task<CurrentUserMenu?> GetAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return null;

        var account = await unitOfWork.UserQueryRepository.GetMenuAsync(userId, ct);
        if (account is null)
            return null;

        // The same key the area policy is pinned to: showing the entry when the gate would refuse it
        // is worse than hiding it, so the rule lives here once rather than in each markup.
        return new CurrentUserMenu(
            FullName: $"{account.FirstName} {account.LastName}".Trim(),
            PhoneNumber: account.PhoneNumber,
            AvatarUrl: account.Avatar,
            CanOpenAdminPanel: user.HasClaim(CustomClaimTypes.Permissions, Permissions.Dashboard.View));
    }
}

/// <summary>
/// The signed-in account as a user menu renders it.
/// </summary>
/// <param name="FullName">First and last name together, as the account stores them.</param>
/// <param name="PhoneNumber">The number the account signs in with; null when it has none.</param>
/// <param name="AvatarUrl">Avatar path/URL; null means the menu shows the fallback icon.</param>
/// <param name="CanOpenAdminPanel">True when the cookie carries the key the admin area requires.</param>
public sealed record CurrentUserMenu(
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    bool CanOpenAdminPanel);
