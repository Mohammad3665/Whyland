using System.Net;
using Microsoft.Extensions.Logging;
using Refit;
using Whyland.Sms.Provider.Abstractions;
using Whyland.Sms.Provider.Contracts;

namespace Whyland.Sms.Provider.Client;

/// <summary>
/// Translates the SMS API into provider-independent <see cref="SmsSendResult"/> values so
/// no Refit or HTTP type reaches business code (s11.3.8.2).
/// <para>
/// Message text is never logged: an OTP must not end up in the log pipeline (s14.2).
/// </para>
/// </summary>
/// <param name="api">Typed HTTP client.</param>
/// <param name="logger">Logger for transport diagnostics.</param>
public sealed class SmsProviderClient(ISmsApiClient api, ILogger<SmsProviderClient> logger)
{
    /// <summary>Sends raw message text.</summary>
    public Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default) =>
        SendCoreAsync(new SmsSendApiRequest(phoneNumber, Message: message), ct);

    /// <summary>Sends a template with parameter values.</summary>
    public Task<SmsSendResult> SendTemplateAsync(
        string phoneNumber,
        string templateCode,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default) =>
        SendCoreAsync(
            new SmsSendApiRequest(
                phoneNumber,
                TemplateCode: templateCode,
                Parameters: new Dictionary<string, object?>(parameters, StringComparer.Ordinal)),
            ct);

    private async Task<SmsSendResult> SendCoreAsync(SmsSendApiRequest request, CancellationToken ct)
    {
        try
        {
            var response = await api.SendAsync(request, ct);

            if (response.Error is not null)
            {
                return FromApiError(response.Error);
            }

            if (response.Content is null)
            {
                logger.LogWarning(
                    "SMS service returned {(Status)}, unreadable body for {Recipient}",
                    (int?)response.StatusCode,
                    request.To);

                return SmsSendResult.Failed(
                    SmsProviderErrorCodes.UnexpectedResponse,
                    "The SMS service responded without a readable body.");
            }

            if (response.Content.Success)
            {
                return SmsSendResult.Sent(response.Content.MessageId, response.Content.Status);
            }

            return SmsSendResult.Failed(
                response.Content.ErrorCode ?? SmsProviderErrorCodes.ServiceError,
                response.Content.ErrorMessage ?? "The SMS service rejected the request.");
        }
        catch (ApiException ex)
        {
            return FromApiError(ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "SMS request to {Recipient} timed out", request.To);

            return SmsSendResult.Indeterminate(
                SmsProviderErrorCodes.TransportFailure,
                "The SMS service did not answer before the timeout; the message may still have been accepted.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "SMS request to {Recipient} could not reach the service", request.To);

            return SmsSendResult.Indeterminate(
                SmsProviderErrorCodes.TransportFailure,
                "The SMS service could not be reached; the message may still have been accepted.");
        }
    }

    private SmsSendResult FromApiError(ApiExceptionBase ex)
    {
        if (ex is not ApiException responseEx)
        {
            logger.LogWarning(ex, "SMS request failed before an HTTP response was received");

            return SmsSendResult.Indeterminate(
                SmsProviderErrorCodes.TransportFailure,
                "The SMS service could not be reached; the message may still have been accepted.");
        }

        var statusCode = (int)responseEx.StatusCode;
        logger.LogWarning("SMS service rejected the request with HTTP {Status}", statusCode);

        return responseEx.StatusCode switch
        {
            HttpStatusCode.Unauthorized => SmsSendResult.Failed(
                SmsProviderErrorCodes.Unauthorized,
                "The SMS API key was rejected."),
            HttpStatusCode.NotFound => SmsSendResult.Failed(
                SmsProviderErrorCodes.ServiceError,
                "The SMS service reported the requested resource as unknown."),
            _ => SmsSendResult.Failed(
                SmsProviderErrorCodes.ServiceError,
                $"The SMS service responded with HTTP {statusCode}."),
        };
    }
}
