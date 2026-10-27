namespace Whyland.Domain.IRepositories.Identity.User;

/// <summary>
/// Command-side repository for the User aggregate (auth-related writes only).
/// The full User CRUD belongs to the admin catalog feature; this contract
/// covers just what the login flow needs (s5.1.3).
/// </summary>
public interface IUserCommandRepository
{
    /// <summary>Stages a new user (auto-registration on first verified OTP); persisted on SaveAsync.</summary>
    Task AddAsync(Entities.Identity.User user, CancellationToken ct = default);
    
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
}