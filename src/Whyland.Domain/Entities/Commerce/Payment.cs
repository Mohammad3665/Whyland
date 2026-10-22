namespace Whyland.Domain.Entities.Commerce;

/// <summary>
/// A single payment attempt against an <see cref="Order"/>, tracked via the gateway (s5.7).
/// </summary>
public class Payment : BaseEntity<Guid>
{
    #region Properties

    /// <summary>
    /// The order being paid.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// The payment channel used (e.g. online gateway).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Amount of this payment attempt.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The outcome of the payment attempt.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Unique reference returned by the payment gateway; null until the bank responds (s5.7).
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// UTC time at which payment succeeded; null until then.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The parent order.
    /// </summary>
    public Order Order { get; set; } = null!;

    #endregion
}
