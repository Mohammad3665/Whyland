using Whyland.Domain.Common.Primitives;

namespace Whyland.Domain.Entities.Commerce;

/// <summary>
/// A discount coupon with activation window and usage-limit rules (s5.6).
/// </summary>
public class Coupon : BaseEntity<Guid>, ISoftDeletable
{
    #region Properties

    /// <summary>
    /// The unique code the customer types at checkout.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// How the discount is computed (percentage or fixed amount).
    /// </summary>
    public CouponType Type { get; set; }

    /// <summary>
    /// The discount magnitude: a percentage when <see cref="Type"/> is Percentage,
    /// otherwise a monetary amount.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Minimum order total required for the coupon to be valid; null means no minimum (s5.6).
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Maximum number of times the coupon may be used across all orders; null means unlimited (s5.6).
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// Number of times the coupon has been used so far.
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// UTC start of the validity window.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// UTC end of the validity window.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Administrators can disable a coupon without deleting its history.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Relations

    /// <summary>
    /// The historical orders that used this coupon.
    /// </summary>
    public ICollection<Order> Orders { get; set; } = [];

    #endregion
}
