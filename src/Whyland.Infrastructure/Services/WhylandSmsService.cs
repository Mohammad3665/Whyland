using Mapster;
using Whyland.Sms.Provider.Client;

namespace Whyland.Infrastructure.Services;

/// <summary>
/// <see cref="ISmsService"/> backed by the standalone Whyland.Sms API (s11.3).
/// The package already hides HTTP and Refit types; this adapter only swaps the
/// result record for the Application-layer contract so business code stays
/// free of provider dependencies.
/// <para>Message text is never logged — an OTP must not reach the log pipeline (s14.2).</para>
/// </summary>
/// <param name="client">HTTP client of the SMS service.</param>
/// <param name="logger">Structured logger.</param>
public sealed class WhylandSmsService(
    SmsProviderClient client,
    ILogger<WhylandSmsService> logger
) : ISmsService
{
    /// <summary>Sends through the SMS service and reports the outcome to the caller.</summary>
    public async Task<SendSmsResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var result = await client.SendAsync(phoneNumber, message, ct);

        if (!result.Success)
        {
            logger.LogWarning(
                "SMS delivery for {Phone} failed with {ErrorCode} (delivery unknown: {DeliveryUnknown})",
                phoneNumber,
                result.ErrorCode,
                result.DeliveryUnknown);
        }

        return result.Adapt<SendSmsResult>();
    }

    /// <summary>Sends a template-based SMS and reports the outcome to the caller.</summary>
    public Task<SendSmsResult> SendTemplateAsync(
        string phoneNumber, string templateCode,
        IReadOnlyDictionary<string, object?> parameters, CancellationToken ct = default)
        => client.SendTemplateAsync(phoneNumber, templateCode, parameters, ct)
            .ContinueWith(t => t.Result.Adapt<SendSmsResult>(), ct);
}
