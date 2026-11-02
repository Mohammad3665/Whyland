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
    public Guid OrderId { get; private set; }

    /// <summary>
    /// The purchased course.
    /// </summary>
    public long CourseId { get; private set; }

    /// <summary>
    /// Number of courses purchased on this line (usually 1 for single-seat courses).
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Price of a single course at purchase time.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Line total: <see cref="UnitPrice"/> × <see cref="Quantity"/>.
    /// </summary>
    public decimal TotalPrice { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The parent order.
    /// </summary>
    public Order Order { get; private set; } = null!;

    /// <summary>
    /// The purchased course.
    /// </summary>
    public Course Course { get; private set; } = null!;

    #endregion
}
