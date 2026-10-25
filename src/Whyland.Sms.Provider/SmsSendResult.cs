namespace Whyland.Sms.Provider;

/// <summary>
/// Codes the package produces itself. Codes coming from the SMS service are passed through
/// unchanged so callers can branch on the documented API contract.
/// </summary>
public static class SmsProviderErrorCodes
{
    /// <summary>The service answered with an HTTP error and no parseable body.</summary>
    public const string ServiceError = "Sms.Provider.ServiceError";

    /// <summary>The service answered, but the payload could not be read as the v1 contract.</summary>
    public const string UnexpectedResponse = "Sms.Provider.UnexpectedResponse";

    /// <summary>No HTTP response arrived (DNS, refused connection, or timeout).</summary>
    public const string TransportFailure = "Sms.Provider.Transport";

    /// <summary>The service rejected the credentials.</summary>
    public const string Unauthorized = "Sms.Provider.Unauthorized";
}

/// <summary>
/// Provider-independent outcome of one send (s11.3.7.1).
/// </summary>
/// <param name="Success">True only when the service confirmed the message was recorded.</param>
/// <param name="MessageId">Identifier returned by the service.</param>
/// <param name="Status">Status string from the service.</param>
/// <param name="ErrorCode">Service error code, or one of <see cref="SmsProviderErrorCodes"/>.</param>
/// <param name="ErrorMessage">Reason a caller can show to a developer or log safely.</param>
/// <param name="DeliveryUnknown">
/// True when the request may still have been processed even though no answer arrived;
/// callers must not assume the message was not sent (s11.3.11).
/// </param>
public sealed record SmsSendResult(
    bool Success,
    string? MessageId = null,
    string? Status = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    bool DeliveryUnknown = false)
{
    /// <summary>A confirmed delivery.</summary>
    public static SmsSendResult Sent(string? messageId, string? status) =>
        new(true, messageId, status);

    /// <summary>A definite rejection.</summary>
    public static SmsSendResult Failed(string errorCode, string errorMessage) =>
        new(false, ErrorCode: errorCode, ErrorMessage: errorMessage);

    /// <summary>A failure where the message may or may not have reached the service.</summary>
    public static SmsSendResult Indeterminate(string errorCode, string errorMessage) =>
        new(false, ErrorCode: errorCode, ErrorMessage: errorMessage, DeliveryUnknown: true);
}
