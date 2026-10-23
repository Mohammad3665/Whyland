namespace Whyland.Application.Common.Interfaces.Services;

/// <summary>
/// Abstraction over sending an SMS, so the login flow never depends on a
/// concrete provider (s11). Phase 1 ships a logging stub; the real provider
/// (Whyland.Sms) is wired in a later phase without touching this contract.
/// </summary>
public interface ISmsService
{
    /// <summary>Sends a raw message to a phone number. Implementations must not log the OTP in production.</summary>
    Task SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}