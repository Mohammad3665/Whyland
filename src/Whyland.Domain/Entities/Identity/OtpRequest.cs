
namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// A one-time password issued for phone-based login, storing only the code's hash (s5.1.4).
/// </summary>
public class OtpRequest : BaseEntity<Guid>
{
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

    #endregion
}
