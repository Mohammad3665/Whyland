using Whyland.Domain.Entities.Authorization;

namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// The authentication-centric user (s5.1.1).
/// Passwords must never be stored in plaintext; only a hash is kept (optional for
/// external-login-only accounts).
/// </summary>
public class User : BaseEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Unique username of the account.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Unique when present. Mandatory before creating a payable order (s5.5).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Unique when present.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Hash of the local password; null for external-login-only accounts.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Inactive users must not authenticate or perform authenticated operations (s5.1.3).
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// The extended, non-authentication profile data (s5.1.2).
    /// </summary>
    public UserProfile? Profile { get; set; }

    /// <summary>
    /// The dedicated instructor information, when the user is an instructor (s5.2).
    /// </summary>
    public InstructorProfile? InstructorProfile { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
