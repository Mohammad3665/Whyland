using Whyland.Domain.Model.Commerce.Coupon.Entities;
using Whyland.Domain.Model.Commerce.Invoice.Entities;
using Whyland.Domain.Model.Commerce.InvoiceItem.Entities;
using Whyland.Domain.Model.Commerce.Order.Entities;
using Whyland.Domain.Model.Commerce.OrderItem.Entities;
using Whyland.Domain.Model.Commerce.Payment.Entities;

namespace Whyland.Domain.Model.Learning.UserCourse.Entities;

/// <summary>
/// The purchase record granting a user permanent access to a course (s4.9). Created only
/// after a successful payment; it is the proof of ownership used by the student classroom.
/// </summary>
public class UserCourse
{
    #region Properties

    /// <summary>
    /// The user who purchased the course.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The purchased course.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The order from which the purchase originated, for bookkeeping and refunds.
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// UTC time of the successful purchase.
    /// </summary>
    public DateTime PurchasedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The owning user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The purchased course.
    /// </summary>
    public Course Course { get; private set; } = null!;

    /// <summary>
    /// The originating order.
    /// </summary>
    public Order Order { get; private set; } = null!;

    #endregion
}
