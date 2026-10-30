namespace Whyland.Domain.Common.Enums;

/// <summary>
/// Payment record lifecycle status (s9.9).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    /// <summary>
    /// Awaiting the gateway result.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment completed successfully; grants course access and issues the invoice.
    /// </summary>
    Successful = 1,

    /// <summary>
    /// The gateway reported a failure.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Cancelled by the business rules (e.g. pending-order expiration).
    /// </summary>
    Cancelled = 3
}
