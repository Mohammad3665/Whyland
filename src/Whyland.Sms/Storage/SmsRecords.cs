namespace Whyland.Sms.Storage;

/// <summary>
/// One message the simulator accepted, held only for the lifetime of the process (s11.1.4).
/// </summary>
/// <param name="MessageId">Public identifier returned to callers.</param>
/// <param name="Recipient">Normalized destination number.</param>
/// <param name="Message">Final rendered text.</param>
/// <param name="Status">One of <see cref="SmsStatus"/>.</param>
/// <param name="CreatedAt">UTC moment the message record was created.</param>
/// <param name="TemplateCode">Template used, when the send was template-based.</param>
/// <param name="SentAt">UTC moment the simulator marked it delivered.</param>
/// <param name="ErrorCode">Documented error code for failed messages.</param>
/// <param name="ErrorMessage">Failure reason for failed messages.</param>
public sealed record SmsMessageRecord(
    string MessageId,
    string Recipient,
    string Message,
    string Status,
    DateTime CreatedAt,
    string? TemplateCode = null,
    DateTime? SentAt = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);

/// <summary>
/// Audit row for a single inbound API call, used by the dashboard to inspect traffic (s11.3.5.2).
/// The payload keeps the message text because reading it is the purpose of the simulator;
/// it is never written to logs.
/// </summary>
/// <param name="RequestId">Correlation id for the inbound call.</param>
/// <param name="Endpoint">Versioned route that handled the call.</param>
/// <param name="Recipient">Normalized recipient when the call carried one.</param>
/// <param name="TemplateCode">Template code when the call referenced one.</param>
/// <param name="Payload">Request body as received.</param>
/// <param name="ReceivedAt">UTC moment the call arrived.</param>
/// <param name="Status">One of <see cref="SmsStatus"/>.</param>
/// <param name="MessageId">Message produced by the call, when one was produced.</param>
/// <param name="ErrorCode">Documented error code, when the call was rejected.</param>
/// <param name="ErrorMessage">Failure reason, when the call was rejected.</param>
public sealed record SmsRequestRecord(
    Guid RequestId,
    string Endpoint,
    string? Recipient,
    string? TemplateCode,
    object? Payload,
    DateTime ReceivedAt,
    string Status,
    string? MessageId = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);
