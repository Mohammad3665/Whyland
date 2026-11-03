
using OrderEntity = Whyland.Domain.Model.Commerce.Order.Entities.Order;

namespace Whyland.Domain.Model.Commerce.Coupon.Entities;

/// <summary>
/// A discount coupon with activation window and usage-limit rules (s5.6).
/// </summary>
public class Coupon : BaseEntity<CouponId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Code"/>; also the database column size.</summary>
    public const int MaxCodeLength = 50;

    #endregion

    #region Properties

    /// <summary>
    /// The unique code the customer types at checkout.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// How the discount is computed (percentage or fixed amount).
    /// </summary>
    public CouponType Type { get; private set; }

    /// <summary>
    /// The discount magnitude: a percentage when <see cref="Type"/> is Percentage,
    /// otherwise a monetary amount.
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Minimum order total required for the coupon to be valid; null means no minimum (s5.6).
    /// </summary>
    public decimal? MinOrderAmount { get; private set; }

    /// <summary>
    /// Maximum number of times the coupon may be used across all orders; null means unlimited (s5.6).
    /// </summary>
    public int? UsageLimit { get; private set; }

    /// <summary>
    /// Number of times the coupon has been used so far.
    /// </summary>
    public int UsedCount { get; private set; }

    /// <summary>
    /// UTC start of the validity window.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// UTC end of the validity window.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Administrators can disable a coupon without deleting its history.
    /// </summary>
    public bool IsActive { get; private set; } = true;

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
    /// The historical orders that used this coupon.
    /// </summary>
    public ICollection<OrderEntity> Orders { get; } = [];

    #endregion
}
