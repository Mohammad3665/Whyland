namespace Whyland.Domain.Entities.Commerce;

/// <summary>
/// A single invoiced course line with its price snapshot, frozen at issue time (s5.8).
/// </summary>
public class InvoiceItem : BaseEntity<Guid>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="ProductName"/>; also the database column size.</summary>
    public const int MaxProductNameLength = 200;

    #endregion

    #region Properties

    /// <summary>
    /// The parent invoice.
    /// </summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>
    /// Snapshot of the invoiced course identifier.
    /// </summary>
    public long CourseId { get; private set; }

    /// <summary>
    /// Snapshot of the course name at issue time.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Number of courses invoiced on this line.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Snapshot of the single-course price.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Discount applied to this line.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Line total after the discount.
    /// </summary>
    public decimal TotalPrice { get; private set; }

    #endregion
}
