using Whyland.Domain.Common.Errors;

namespace Whyland.Domain.Common.Errors;

/// <summary>
/// Failures that belong to no aggregate because they come from the surrounding infrastructure —
/// delivery, storage. They still carry a stable code so the wording is resolved like every other error,
/// and they never name the failing provider or its response.
/// </summary>
public static class SystemErrors
{
    /// <summary>
    /// A message did not reach the SMS provider, so anything depending on it (an OTP) was not delivered.
    /// Retryable; the caller still governs the resend cooldown.
    /// </summary>
    public static DomainError SmsNotSent() =>
        DomainError.For("System", nameof(SmsNotSent), ErrorType.Unexpected);

    /// <summary>
    /// The uploaded file is not an image the storage layer accepts: an unsupported extension or format,
    /// a mismatched signature, or dimensions beyond the configured limits (s8.3.1).
    /// </summary>
    public static DomainError InvalidImage() =>
        DomainError.For("System", nameof(InvalidImage), ErrorType.Validation);

    /// <summary>The upload is larger than the configured ceiling (s8.3.1, s15.1.6).</summary>
    public static DomainError ImageTooLarge() =>
        DomainError.For("System", nameof(ImageTooLarge), ErrorType.Validation);

    /// <summary>The image could not be written or removed; retryable and masked behind a generic message.</summary>
    public static DomainError ImageStorageFailed() =>
        DomainError.For("System", nameof(ImageStorageFailed), ErrorType.Unexpected);
}
