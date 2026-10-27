using Whyland.Domain.Entities.Authorization;

namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// The authentication-centric user (s5.1.1).
/// Passwords must never be stored in plaintext; only a hash is kept (optional for
/// external-login-only accounts).
/// </summary>
public class User : BaseEntity<Guid>
{
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Unique when present. It is the identifier password sign-in resolves by, the number an OTP is
    /// sent to, and the channel a forgotten password is recovered through (s5.1.1, s5.4). It is also
    /// mandatory before creating a payable order (s5.5).
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

    /// <summary>
    /// UTC time of the user's most recent successful login; null before the first login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Consecutive failed password logins, cleared by a successful one (s15.1.4).
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// UTC time until which password login is blocked after too many failures; null when not locked.
    /// OTP and external login are not affected — they have their own throttling.
    /// </summary>
    public DateTime? LockoutUntilUtc { get; set; }

    /// <summary>
    /// The extended, non-authentication profile data (s5.1.2).
    /// </summary>
    public UserProfile? Profile { get; set; }

    /// <summary>
    /// The dedicated instructor information, when the user is an instructor (s5.2).
    /// </summary>
    public InstructorProfile? InstructorProfile { get; set; }

    /// <summary>
    /// The roles assigned to this user.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
