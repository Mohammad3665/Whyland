namespace Whyland.Application.Common.Interfaces.Services;

/// <summary>
/// Outcome of one SMS send, expressed without provider or HTTP types so callers can branch
/// on it (s11.3.7.1).
/// </summary>
/// <param name="Success">True only when the message was accepted for delivery.</param>
/// <param name="MessageId">Provider-side identifier of the message.</param>
/// <param name="Status">Delivery status reported by the provider.</param>
/// <param name="ErrorCode">Stable machine-readable code of the failure.</param>
/// <param name="ErrorMessage">Developer-readable reason; never shown raw to end users.</param>
/// <param name="DeliveryUnknown">
/// True when the request may still have been accepted even though no answer arrived
/// (timeout or dropped connection) — callers must not assume the user got nothing (s11.3.11).
/// </param>
public sealed record SendSmsResult(
    bool Success,
    string? MessageId = null,
    string? Status = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    bool DeliveryUnknown = false)
{
    /// <summary>The message was accepted by the provider.</summary>
    public static SendSmsResult Sent(string? messageId = null, string? status = null) =>
        new(true, messageId, status);

    /// <summary>The provider definitely rejected the message.</summary>
    public static SendSmsResult Failed(string errorCode, string errorMessage) =>
        new(false, ErrorCode: errorCode, ErrorMessage: errorMessage);

    /// <summary>No answer arrived; the message may or may not have been accepted.</summary>
    public static SendSmsResult Indeterminate(string errorCode, string errorMessage) =>
        new(false, ErrorCode: errorCode, ErrorMessage: errorMessage, DeliveryUnknown: true);
}
