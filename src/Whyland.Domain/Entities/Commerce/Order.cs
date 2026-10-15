using Whyland.Domain.Entities.Identity;

namespace Whyland.Domain.Entities.Commerce;

/// <summary>
/// A payable order of one or more courses, created by the purchasing user (s5.5).
/// Money fields are snapshots; per-course prices are held by <see cref="OrderItem"/>.
/// </summary>
public class Order : BaseEntity<Guid>
{
    #region Properties

    /// <summary>
    /// The user who placed the order (s5.5).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Total payable amount of the order, after any coupon discount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The current state of the order in the purchase flow.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// UTC time at which the order was fully paid; null until payment succeeds.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// UTC time after which a still-pending order expires and payments are refused.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// The coupon applied to this order; null when no coupon was used (s5.6).
    /// </summary>
    public Guid? CouponId { get; set; }

    /// <summary>
    /// The amount actually discounted by the coupon at purchase time; 0 without a coupon.
    /// </summary>
    public decimal CouponDiscountAmount { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The purchasing user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The applied coupon, when any.
    /// </summary>
    public Coupon? Coupon { get; set; }

    /// <summary>
    /// The individual course lines of this order.
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = [];

    /// <summary>
    /// All payment attempts made against this order.
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = [];

    #endregion
}
