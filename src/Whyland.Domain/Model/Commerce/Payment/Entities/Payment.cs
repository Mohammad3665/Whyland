
using OrderEntity = Whyland.Domain.Model.Commerce.Order.Entities.Order;

namespace Whyland.Domain.Model.Commerce.Payment.Entities;

/// <summary>
/// A single payment attempt against an <see cref="OrderEntity"/>, tracked via the gateway (s5.7).
/// </summary>
public class Payment : BaseEntity<PaymentId>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="TrackingNumber"/>; also the database column size.</summary>
    public const int MaxTrackingNumberLength = 100;

    #endregion

    #region Properties

    /// <summary>
    /// The order being paid.
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// The payment channel used (e.g. online gateway).
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Amount of this payment attempt.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// The outcome of the payment attempt.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Unique reference returned by the payment gateway; null until the bank responds (s5.7).
    /// </summary>
    public string? TrackingNumber { get; private set; }

    /// <summary>
    /// UTC time at which payment succeeded; null until then.
    /// </summary>
    public DateTime? PaidAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The parent order.
    /// </summary>
    public OrderEntity Order { get; private set; } = null!;

    #endregion
}
