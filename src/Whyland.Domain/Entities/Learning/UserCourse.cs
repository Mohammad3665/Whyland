using Whyland.Domain.Entities.Catalog;
using Whyland.Domain.Entities.Commerce;
using Whyland.Domain.Entities.Identity;

namespace Whyland.Domain.Entities.Learning;

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
    public Guid UserId { get; set; }

    /// <summary>
    /// The purchased course.
    /// </summary>
    public long CourseId { get; set; }

    /// <summary>
    /// The order from which the purchase originated, for bookkeeping and refunds.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// UTC time of the successful purchase.
    /// </summary>
    public DateTime PurchasedAt { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The owning user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The purchased course.
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// The originating order.
    /// </summary>
    public Order Order { get; set; } = null!;

    #endregion
}
