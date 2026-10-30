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
}
