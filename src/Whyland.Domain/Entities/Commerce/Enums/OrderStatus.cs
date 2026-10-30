namespace Whyland.Domain.Common.Enums;

/// <summary>
/// Order lifecycle status (s9.6).
/// Transitions are controlled by application business rules only.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    /// <summary>
    /// Awaiting payment; expires after the configured pending duration (s9.6.1).
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment succeeded; course access has been granted.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Cancelled by the user/business rules (e.g. pending-order expiration).
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 3
}
