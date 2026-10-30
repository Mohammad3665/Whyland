using Whyland.Domain.Entities.Authorization;

namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// The authentication-centric user (s5.1.1).
/// Passwords must never be stored in plaintext; only a hash is kept (optional for
/// external-login-only accounts).
/// <para>
/// Every state change goes through a method that names the fact it represents — an account is created
/// with <see cref="Create"/> and a password is replaced with <see cref="SetPassword"/> — so no caller can
/// write one field and forget the counter or the lockout that belongs to it.
/// </para>
/// </summary>
public class User : BaseEntity<Guid>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Email"/>; also the database column size.</summary>
    public const int MaxEmailLength = 256;

    /// <summary>Maximum allowed length of <see cref="FirstName"/>; also the database column size.</summary>
    public const int MaxFirstNameLength = 100;

    /// <summary>Maximum allowed length of <see cref="LastName"/>; also the database column size.</summary>
    public const int MaxLastNameLength = 100;

    /// <summary>Maximum allowed length of <see cref="PasswordHash"/>; also the database column size.</summary>
    public const int MaxPasswordHashLength = 512;

    /// <summary>Maximum allowed length of <see cref="PhoneNumber"/>; also the database column size.</summary>
    public const int MaxPhoneNumberLength = 20;

    #endregion

    #region Properties

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Unique when present. It is the identifier password sign-in resolves by, the number an OTP is
    /// sent to, and the channel a forgotten password is recovered through (s5.1.1, s5.4). It is also
    /// mandatory before creating a payable order (s5.5).
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Unique when present.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Hash of the local password; null for external-login-only accounts.
    /// </summary>
    public string? PasswordHash { get; private set; }

    /// <summary>
    /// Inactive users must not authenticate or perform authenticated operations (s5.1.3).
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// UTC time of the user's most recent successful login; null before the first login.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Consecutive failed password logins, cleared by a successful one (s15.1.4).
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// UTC time until which password login is blocked after too many failures; null when not locked.
    /// OTP and external login are not affected — they have their own throttling.
    /// </summary>
    public DateTime? LockoutUntilUtc { get; private set; }

    /// <summary>
    /// The extended, non-authentication profile data (s5.1.2). Filled by the persistence layer.
    /// </summary>
    public UserProfile? Profile { get; private set; }

    /// <summary>
    /// The dedicated instructor information, when the user is an instructor (s5.2). Filled by the
    /// persistence layer.
    /// </summary>
    public InstructorProfile? InstructorProfile { get; private set; }

    /// <summary>
    /// The roles assigned to this user. Assignments are added and removed by the persistence layer.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create accounts through <see cref="Create"/>.
    /// </summary>
    private User() { }

    /// <summary>
    /// Opens an account from already-normalised values: the number is the one the caller verified, the
    /// hash the one the caller produced (an external-login account has none), and neither is re-checked here.
    /// </summary>
    /// <param name="firstName">The given name.</param>
    /// <param name="lastName">The family name.</param>
    /// <param name="phoneNumber">The sign-in number, already verified by the calling flow.</param>
    /// <param name="email">The e-mail address, when the flow knows one.</param>
    /// <param name="passwordHash">The hashed local password, or null when the account has no password.</param>
    /// <param name="isActive">Whether the account may sign in; an administrator can open a disabled one.</param>
    public static User Create(
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? email = null,
        string? passwordHash = null,
        bool isActive = true)
        => new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = isActive,
        };

    #endregion

    #region Behaviour

    /// <summary>
    /// Rewrites the account details from the admin edit form. Uniqueness of the number and the e-mail is
    /// checked by the caller against the store, not here — an entity cannot see the other accounts.
    /// </summary>
    /// <param name="firstName">The given name.</param>
    /// <param name="lastName">The family name.</param>
    /// <param name="phoneNumber">The sign-in number, or null to clear it.</param>
    /// <param name="email">The e-mail address, or null to clear it.</param>
    /// <param name="isActive">Whether the account may sign in.</param>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? email, bool isActive)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        IsActive = isActive;
    }

    /// <summary>
    /// Attaches a sign-in number the caller already proved — through an OTP code while the owner sets a
    /// password, or through the configured value while the super admin account is seeded.
    /// </summary>
    /// <param name="phoneNumber">The number in the form the store keeps it.</param>
    public void BindPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;

    /// <summary>
    /// Enables or disables the account; a disabled account cannot authenticate (s5.1.3).
    /// </summary>
    /// <param name="isActive">The state to move to.</param>
    public void SetActive(bool isActive) => IsActive = isActive;

    /// <summary>
    /// Stores a new local password and clears the failed-login state, because the credential that
    /// produced it has just been replaced.
    /// </summary>
    /// <param name="passwordHash">The hash of the new password; never the password itself.</param>
    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        FailedLoginAttempts = 0;
        LockoutUntilUtc = null;
    }

    /// <summary>
    /// Counts one failed password attempt, locking the account when the threshold is reached (s15.1.4).
    /// </summary>
    /// <param name="maxFailedAttempts">How many failures are tolerated before the lockout.</param>
    /// <param name="lockoutMinutes">How long the account stays blocked once it is reached.</param>
    /// <param name="utcNow">The current UTC time the lockout window is measured from.</param>
    public void RecordFailedAttempt(int maxFailedAttempts, int lockoutMinutes, DateTime utcNow)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts < maxFailedAttempts)
            return;

        LockoutUntilUtc = utcNow.AddMinutes(lockoutMinutes);
        FailedLoginAttempts = 0;
    }

    /// <summary>
    /// Marks a successful sign-in: the timestamp moves and the failure state is cleared.
    /// </summary>
    /// <param name="utcNow">The current UTC time.</param>
    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
        FailedLoginAttempts = 0;
        LockoutUntilUtc = null;
    }

    /// <summary>Whether password sign-in is currently blocked by too many failures.</summary>
    /// <param name="utcNow">The current UTC time compared against the lockout window.</param>
    public bool IsLockedOut(DateTime utcNow) => LockoutUntilUtc is { } until && until > utcNow;

    #endregion
}
