namespace Whyland.Sms.Contracts.V1;

/// <summary>
/// v1 request body for <c>POST /api/v1/sms/send</c>. Exactly one delivery mode is
/// accepted: raw <see cref="Message"/> text, or <see cref="TemplateCode"/> with
/// <see cref="Parameters"/> (s11.3.4.3).
/// </summary>
public sealed record SendSmsRequest
{
    /// <summary>Recipient in provider style, e.g. 09120000000 or +989120000000.</summary>
    public string? To { get; init; }

    /// <summary>Final message text, used when no template is referenced.</summary>
    public string? Message { get; init; }

    /// <summary>Unique template code, e.g. SMS-OTP-001.</summary>
    public string? TemplateCode { get; init; }

    /// <summary>Values substituted into the template; keys must match the template parameters.</summary>
    public Dictionary<string, JsonElement>? Parameters { get; init; }
}

/// <summary>
/// v1 response body, shaped like a real provider answer: a success flag plus the
/// generated message id, or an error code and a message safe to show to developers (s11.1.2).
/// </summary>
public sealed record SendSmsResponse
{
    /// <summary>True when the simulator recorded the message.</summary>
    public bool Success { get; init; }

    /// <summary>Generated identifier of the recorded message; null on failure.</summary>
    public string? MessageId { get; init; }

    /// <summary>One of <see cref="SmsStatus"/> values.</summary>
    public string? Status { get; init; }

    /// <summary>Stable machine-readable error code; null on success.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Human-readable failure reason; never contains the API key.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Successful send carrying the new message id.</summary>
    public static SendSmsResponse Sent(string messageId) =>
        new() { Success = true, MessageId = messageId, Status = SmsStatus.Sent };

    /// <summary>Rejected send carrying a documented error code.</summary>
    public static SendSmsResponse Failed(string errorCode, string errorMessage) =>
        new() { Success = false, Status = SmsStatus.Failed, ErrorCode = errorCode, ErrorMessage = errorMessage };
}
