namespace Whyland.Domain.Model.Identity.OtpRequest.Entities;

/// <summary>
/// A one-time password issued for phone-based login, storing only the code's hash (s5.1.4).
/// </summary>
public class OtpRequest : BaseEntity<OtpRequestId>
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
    public string PhoneNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Hash of the issued code; the plaintext code is never persisted (s5.1.4).
    /// </summary>
    public string CodeHash { get; private set; } = string.Empty;

    /// <summary>
    /// Number of failed verification attempts so far.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// Maximum number of verification attempts before the code is exhausted.
    /// </summary>
    public int MaxAttempts { get; private set; }

    /// <summary>
    /// UTC time at which the code becomes invalid.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Set once the code has been verified successfully.
    /// </summary>
    public bool IsUsed { get; private set; }

    /// <summary>
    /// What the code proves. Login codes and phone-binding codes are never interchangeable.
    /// </summary>
    public OtpPurpose Purpose { get; private set; } = OtpPurpose.Login;

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — issue codes through <see cref="Create"/>.
    /// </summary>
    private OtpRequest() { }

    /// <summary>
    /// Issues a fresh, unused code out of values the calling flow already produced: the hash is the one
    /// the caller hashed — the plaintext never reaches this entity — and the expiry the instant the
    /// caller measured from its own clock.
    /// </summary>
    /// <param name="phoneNumber">The number in the form the store keeps it.</param>
    /// <param name="codeHash">The hash of the code, never the code itself.</param>
    /// <param name="maxAttempts">How many verifications are allowed before the code is exhausted.</param>
    /// <param name="expiresAt">The UTC instant from which the code stops being usable.</param>
    /// <param name="purpose">What the code proves.</param>
    public static OtpRequest Create(
        string phoneNumber,
        string codeHash,
        int maxAttempts,
        DateTime expiresAt,
        OtpPurpose purpose)
    {
        var request = new OtpRequest { Id = OtpRequestId.From(Guid.NewGuid()) };

        request.Apply(phoneNumber, codeHash, maxAttempts, expiresAt, purpose);

        return request;
    }

    /// <summary>
    /// Assigns the values the calling flow already produced. Nothing is validated here: the hash and
    /// the expiry are the caller's own work, and the store enforces the column sizes.
    /// </summary>
    /// <param name="phoneNumber">The number in the form the store keeps it.</param>
    /// <param name="codeHash">The hash of the code, never the code itself.</param>
    /// <param name="maxAttempts">How many verifications are allowed before the code is exhausted.</param>
    /// <param name="expiresAt">The UTC instant from which the code stops being usable.</param>
    /// <param name="purpose">What the code proves.</param>
    private void Apply(
        string phoneNumber,
        string codeHash,
        int maxAttempts,
        DateTime expiresAt,
        OtpPurpose purpose)
    {
        PhoneNumber = phoneNumber;
        CodeHash = codeHash;
        AttemptCount = 0;
        MaxAttempts = maxAttempts;
        ExpiresAt = expiresAt;
        IsUsed = false;
        Purpose = purpose;
    }

    #endregion

    #region Behaviour

    /// <summary>
    /// Burns the code so it can never authenticate again: after a successful verification, and also
    /// when the delivery failed and the sender cannot prove the message arrived (s11.3.11).
    /// </summary>
    public void MarkUsed() => IsUsed = true;

    /// <summary>
    /// Counts one failed verification and burns the code when the attempts run out, so guessing is
    /// limited to exactly <see cref="MaxAttempts"/> tries with no fresh window after them.
    /// </summary>
    public void RegisterFailedAttempt()
    {
        AttemptCount++;

        if (AttemptCount >= MaxAttempts)
            IsUsed = true;
    }

    #endregion
}
