
using CouponEntity = Whyland.Domain.Model.Commerce.Coupon.Entities.Coupon;
using OrderItemEntity = Whyland.Domain.Model.Commerce.OrderItem.Entities.OrderItem;
using PaymentEntity = Whyland.Domain.Model.Commerce.Payment.Entities.Payment;

namespace Whyland.Domain.Model.Commerce.Order.Entities;

/// <summary>
/// A payable order of one or more courses, created by the purchasing user (s5.5).
/// Money fields are snapshots; per-course prices are held by <see cref="OrderItemEntity"/>.
/// </summary>
public class Order : BaseEntity<OrderId>, ISoftDeletable
{
    #region Properties

    /// <summary>
    /// The user who placed the order (s5.5).
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Total payable amount of the order, after any coupon discount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// The current state of the order in the purchase flow.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// UTC time at which the order was fully paid; null until payment succeeds.
    /// </summary>
    public DateTime? PaidAt { get; private set; }

    /// <summary>
    /// UTC time after which a still-pending order expires and payments are refused.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// The coupon applied to this order; null when no coupon was used (s5.6).
    /// </summary>
    public CouponId? CouponId { get; private set; }

    /// <summary>
    /// The amount actually discounted by the coupon at purchase time; 0 without a coupon.
    /// </summary>
    public decimal CouponDiscountAmount { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public UserId? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The purchasing user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The applied coupon, when any.
    /// </summary>
    public CouponEntity? Coupon { get; private set; }

    /// <summary>
    /// The individual course lines of this order.
    /// </summary>
    public ICollection<OrderItemEntity> OrderItems { get; } = [];

    /// <summary>
    /// All payment attempts made against this order.
    /// </summary>
    public ICollection<PaymentEntity> Payments { get; } = [];

    #endregion
}
