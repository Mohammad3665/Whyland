namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The kind of discount a coupon grants at checkout (percentage or fixed amount).
/// </summary>
public enum CouponType
{
    /// <summary>
    /// The coupon discounts the order by a percentage of its total.
    /// </summary>
    Percentage,

    /// <summary>
    /// The coupon discounts the order by a fixed monetary amount.
    /// </summary>
    FixedAmount,
}
