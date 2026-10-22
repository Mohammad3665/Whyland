namespace Whyland.Domain.Entities.Commerce;

/// <summary>
/// A single purchased course line inside an <see cref="Order"/>, holding the price snapshot.
/// </summary>
public class OrderItem : BaseEntity<long>
{
    #region Properties

    /// <summary>
    /// The order this line belongs to.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// The purchased course.
    /// </summary>
    public long CourseId { get; set; }

    /// <summary>
    /// Number of courses purchased on this line (usually 1 for single-seat courses).
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price of a single course at purchase time.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line total: <see cref="UnitPrice"/> × <see cref="Quantity"/>.
    /// </summary>
    public decimal TotalPrice { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The parent order.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// The purchased course.
    /// </summary>
    public Course Course { get; set; } = null!;

    #endregion
}
