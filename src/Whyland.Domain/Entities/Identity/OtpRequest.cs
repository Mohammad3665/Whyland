namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// A one-time password issued for phone-based login, storing only the code's hash (s5.1.4).
/// </summary>
public class OtpRequest : BaseEntity<Guid>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="CodeHash"/>; also the database column size.</summary>
    public const int MaxCodeHashLength = 256;

    /// <summary>Maximum allowed length of <see cref="PhoneNumber"/>; also the database column size.</summary>
    public const int MaxPhoneNumberLength = 20;

    #endregion

    #region Properties

    /// <summary>
    /// The phone number the code was sent to.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the issued code; the plaintext code is never persisted (s5.1.4).
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Number of failed verification attempts so far.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Maximum number of verification attempts before the code is exhausted.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// UTC time at which the code becomes invalid.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Set once the code has been verified successfully.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// What the code proves. Login codes and phone-binding codes are never interchangeable.
    /// </summary>
    public OtpPurpose Purpose { get; set; } = OtpPurpose.Login;

    #endregion
}
