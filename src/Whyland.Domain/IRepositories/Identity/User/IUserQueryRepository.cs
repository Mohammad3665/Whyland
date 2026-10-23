namespace Whyland.Domain.IRepositories.Identity.User;

/// <summary>
/// Query-side repository for the User aggregate.
/// Backed by EF Core for now; may move to Dapper as read paths grow.
/// </summary>
public interface IUserQueryRepository
{
    /// <summary>Returns the tracked, active, non-deleted user with the given phone, or null.</summary>
    Task<Entities.Identity.User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Returns the effective permission keys of a user (union of all its roles'
    /// permissions). These keys become the "permissions" claims (s6.4).
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the avatar path/URL of the user's profile, or null when no
    /// profile exists or no avatar has been set.
    /// </summary>
    Task<string?> GetAvatarAsync(Guid userId, CancellationToken ct = default);
}