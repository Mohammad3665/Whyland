namespace Whyland.Domain.IRepositories.Identity.User;

/// <summary>
/// Command-side repository for the User aggregate: the writes the login flow needs (s5.1.3)
/// plus the account mutations made from the admin users page.
/// </summary>
public interface IUserCommandRepository
{
    /// <summary>Stages a new user (auto-registration on first verified OTP); persisted on SaveAsync.</summary>
    Task AddAsync(Entities.Identity.User user, CancellationToken ct = default);

    /// <summary>
    /// Replaces a user's role assignments with <paramref name="roleIds"/>; persisted on SaveAsync.
    /// Roles are the only authorization mechanism, so the assignment is edited as a whole set (s6.4).
    /// </summary>
    Task ReplaceRolesAsync(Guid userId, IReadOnlyCollection<long> roleIds, CancellationToken ct = default);
    
    /// <summary>Stamps LastLoginAt on the tracked user; persisted on SaveAsync.</summary>
    void TouchLastLogin(Entities.Identity.User user);

    /// <summary>
    /// Returns the tracked, non-deleted user whose phone number equals <paramref name="phoneNumber"/>,
    /// including deactivated accounts so the caller can tell "no such account" from "account disabled".
    /// Password sign-in and recovery resolve accounts by number only (s5.1.1, s5.4).
    /// </summary>
    Task<Entities.Identity.User?> GetTrackedByPhoneAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Returns the tracked, non-deleted user that already owns <paramref name="email"/>, so an
    /// address released by an external provider is never attached to a second account.
    /// </summary>
    Task<Entities.Identity.User?> GetTrackedByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Returns the tracked, non-deleted user with the given id, or null.</summary>
    Task<Entities.Identity.User?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns true when the account holds the role named <paramref name="roleName"/>, so the
    /// admin page can refuse to lock itself out of the panel.
    /// </summary>
    Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
}