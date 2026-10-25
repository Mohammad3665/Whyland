namespace Whyland.Application.Common.Interfaces.Services;

/// <summary>
/// Abstraction over sending an SMS, so the login flow never depends on a
/// concrete provider (s11). Phase 1 ships a logging stub; the real provider
/// (Whyland.Sms) is wired in a later phase without touching this contract.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends a raw message to a phone number and reports the outcome, so callers can react
    /// to a failed delivery (s11.3.7). Implementations must not log the OTP in production.
    /// </summary>
    Task<SendSmsResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default);

    /// <summary>
    /// Sends a template-based message to a phone number and reports the outcome, so callers
    /// can react to a failed delivery (s11.3.7). <paramref name="parameters"/> supplies the
    /// placeholders the provider expects for <paramref name="templateCode"/>. Implementations
    /// must not log the parameters in production — they may contain an OTP (s14.2).
    /// </summary>
    Task<SendSmsResult> SendTemplateAsync(
        string phoneNumber, string templateCode,
        IReadOnlyDictionary<string, object?> parameters, CancellationToken ct = default);
}
