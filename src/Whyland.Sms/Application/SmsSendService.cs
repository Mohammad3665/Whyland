using System.Globalization;
using Whyland.Sms.Templates;

namespace Whyland.Sms.Application;

/// <summary>
/// HTTP status plus response body produced for one send call, so the endpoint layer stays
/// a thin mapper.
/// </summary>
/// <param name="StatusCode">Status the API should return.</param>
/// <param name="Response">Provider-style body (s11.1.2).</param>
public sealed record SendOutcome(int StatusCode, SendSmsResponse Response)
{
    /// <summary>Materializes the outcome as a JSON HTTP result.</summary>
    public IResult ToResult() => Results.Json(Response, statusCode: StatusCode);
}

/// <summary>
/// Validates a v1 send request, renders it (raw text or template), and records both the
/// inbound request and the produced message in the in-memory store (s11.1.4, s11.3.4.3).
/// </summary>
/// <param name="store">In-memory traffic store.</param>
/// <param name="renderer">Template placeholder renderer.</param>
/// <param name="options">Simulator limits.</param>
/// <param name="timeProvider">UTC clock.</param>
/// <param name="audience">Watchers notified as soon as a message is stored.</param>
public sealed class SmsSendService(
    ISmsStore store,
    SmsTemplateRenderer renderer,
    IOptions<SmsServiceOptions> options,
    TimeProvider timeProvider,
    ISmsMessageAudience audience)
{
    private long _sequence;

    /// <summary>Processes one send request and returns the status/body to hand back.</summary>
    public SendOutcome Send(SendSmsRequest request, string endpoint, Guid requestId)
    {
        var recipient = PhoneNumber.Normalize(request.To);
        var hasRawMessage = !string.IsNullOrWhiteSpace(request.Message);
        var hasTemplate = !string.IsNullOrWhiteSpace(request.TemplateCode);

        if (recipient is null)
        {
            return Reject(requestId, endpoint, request, null, SmsErrorCodes.InvalidRecipient,
                "Recipient must be an Iranian mobile number, e.g. 09120000000 or +989120000000.",
                StatusCodes.Status400BadRequest);
        }

        if (!hasRawMessage && !hasTemplate)
        {
            return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.MissingContent,
                "Provide either 'message' or 'templateCode'.", StatusCodes.Status400BadRequest);
        }

        if (hasRawMessage && hasTemplate)
        {
            return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.AmbiguousContent,
                "Provide 'message' or 'templateCode', not both.", StatusCodes.Status400BadRequest);
        }

        string? text;
        string? templateCode = null;

        if (hasTemplate)
        {
            var template = store.FindTemplate(request.TemplateCode!);

            if (template is null)
            {
                return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.TemplateNotFound,
                    $"No SMS template is registered with code '{request.TemplateCode}'.",
                    StatusCodes.Status404NotFound);
            }

            if (!template.IsActive)
            {
                return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.TemplateDisabled,
                    $"Template '{template.Code}' is inactive.", StatusCodes.Status400BadRequest);
            }

            var rendered = renderer.Render(template, request.Parameters);

            if (!rendered.IsSuccess)
            {
                return Reject(requestId, endpoint, request, recipient, rendered.ErrorCode!,
                    rendered.ErrorMessage!, StatusCodes.Status400BadRequest, template.Code);
            }

            templateCode = template.Code;
            text = rendered.Content;
        }
        else
        {
            text = request.Message;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.EmptyMessage,
                "The rendered message is empty.", StatusCodes.Status400BadRequest, templateCode);
        }

        if (text.Length > options.Value.MaxMessageLength)
        {
            return Reject(requestId, endpoint, request, recipient, SmsErrorCodes.MessageTooLong,
                $"The message is {text.Length} characters; the limit is {options.Value.MaxMessageLength}.",
                StatusCodes.Status400BadRequest, templateCode);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var messageId = NextMessageId(now);

        var record = new SmsMessageRecord(
            MessageId: messageId,
            Recipient: recipient,
            Message: text,
            Status: SmsStatus.Sent,
            CreatedAt: now,
            TemplateCode: templateCode,
            SentAt: now);

        store.AddMessage(record);
        audience.MessageStored(record);

        store.AddRequest(new SmsRequestRecord(
            RequestId: requestId,
            Endpoint: endpoint,
            Recipient: recipient,
            TemplateCode: templateCode,
            Payload: request,
            ReceivedAt: now,
            Status: SmsStatus.Sent,
            MessageId: messageId));

        return new SendOutcome(StatusCodes.Status200OK, SendSmsResponse.Sent(messageId));
    }

    private SendOutcome Reject(
        Guid requestId,
        string endpoint,
        SendSmsRequest request,
        string? recipient,
        string errorCode,
        string errorMessage,
        int statusCode,
        string? templateCode = null)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        store.AddRequest(new SmsRequestRecord(
            RequestId: requestId,
            Endpoint: endpoint,
            Recipient: recipient,
            TemplateCode: templateCode,
            Payload: request,
            ReceivedAt: now,
            Status: SmsStatus.Failed,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage));

        return new SendOutcome(statusCode, SendSmsResponse.Failed(errorCode, errorMessage));
    }

    private string NextMessageId(DateTime now) =>
        $"sms_{now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}_"
        + Interlocked.Increment(ref _sequence).ToString("D6", CultureInfo.InvariantCulture);
}

/// <summary>
/// Normalizes Iranian mobile numbers to the provider form <c>09xxxxxxxxx</c>.
/// </summary>
public static class PhoneNumber
{
    /// <summary>Returns the normalized number, or null when the input is not a mobile number.</summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());

        // 0098 / 98 country codes become the local 09 form; a bare 9xxxxxxxxx gets its leading zero.
        if (digits.StartsWith("0098", StringComparison.Ordinal))
        {
            digits = "0" + digits[4..];
        }
        else if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length == 12)
        {
            digits = "0" + digits[2..];
        }
        else if (digits.Length == 10 && digits.StartsWith("9", StringComparison.Ordinal))
        {
            digits = "0" + digits;
        }

        return digits is ['0', '9', ..] && digits.Length == 11 ? digits : null;
    }
}
