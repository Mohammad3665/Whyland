namespace Whyland.Infrastructure.Services;

/// <summary>
/// Development stub for ISmsService: writes the message to the structured log
/// instead of a real provider (s11.2). Replace with Whyland.Sms.Provider later;
/// no calling code changes because only the ISmsService contract is depended on.
/// </summary>
/// <param name="logger">Structured logger.</param>
public sealed class StubSmsService(ILogger<StubSmsService> logger) : ISmsService
{
    /// <summary>Logs the OTP instead of sending it. In production this logs secrets — swap it out.</summary>
    public Task<SendSmsResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // Development only: surfaces the OTP in logs/console so you can log in.
        logger.LogInformation("STUB SMS → {Phone}: {Message}", phoneNumber, message);
        return Task.FromResult(SendSmsResult.Sent(status: "Stubbed"));
    }

    /// <summary>Logs the template code and parameters instead of sending them.</summary>
    public Task<SendSmsResult> SendTemplateAsync(string phoneNumber, string templateCode, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct = default)
    {
        // Development only: surfaces the OTP parameters in logs/console so you can log in.
        logger.LogInformation(
            "STUB SMS TEMPLATE → {Phone} [{TemplateCode}]: {@Parameters}",
            phoneNumber,
            templateCode,
            parameters);
        return Task.FromResult(SendSmsResult.Sent(status: "Stubbed"));
    }
}
