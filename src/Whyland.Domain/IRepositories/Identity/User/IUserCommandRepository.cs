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
}