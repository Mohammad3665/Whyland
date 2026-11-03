
using InvoiceItemEntity = Whyland.Domain.Model.Commerce.InvoiceItem.Entities.InvoiceItem;
using OrderEntity = Whyland.Domain.Model.Commerce.Order.Entities.Order;

namespace Whyland.Domain.Model.Commerce.Invoice.Entities;

/// <summary>
/// A printable purchase invoice holding immutable snapshots of customer, money and coupon data (s5.8).
/// Properties are deliberately settable only by the entity itself so snapshots stay unmodifiable.
/// </summary>
public class Invoice : BaseEntity<InvoiceId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="CouponCode"/>; also the database column size.</summary>
    public const int MaxCouponCodeLength = 50;

    /// <summary>Maximum allowed length of <see cref="CustomerEmail"/>; also the database column size.</summary>
    public const int MaxCustomerEmailLength = 256;

    /// <summary>Maximum allowed length of <see cref="CustomerName"/>; also the database column size.</summary>
    public const int MaxCustomerNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="CustomerPhone"/>; also the database column size.</summary>
    public const int MaxCustomerPhoneLength = 20;

    /// <summary>Maximum allowed length of <see cref="InvoiceNumber"/>; also the database column size.</summary>
    public const int MaxInvoiceNumberLength = 50;

    #endregion

    #region Properties

    /// <summary>
    /// The system-wide unique, human-readable invoice number.
    /// </summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// UTC date the invoice was generated.
    /// </summary>
    public DateTime InvoiceDate { get; private set; }

    /// <summary>
    /// The order the invoice was issued for.
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// The successful payment that triggered the invoice.
    /// </summary>
    public PaymentId PaymentId { get; private set; }

    /// <summary>
    /// The customer the invoice was issued to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Snapshot of the customer's full name at issue time.
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// Snapshot of the customer's phone number at issue time.
    /// </summary>
    public string CustomerPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Snapshot of the customer's email at issue time.
    /// </summary>
    public string CustomerEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Sum of line totals before any discounts.
    /// </summary>
    public decimal Subtotal { get; private set; }

    /// <summary>
    /// Total discount applied, including the coupon discount.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Snapshot of the coupon code; null when no coupon was used.
    /// </summary>
    public string? CouponCode { get; private set; }

    /// <summary>
    /// Snapshot of the coupon discount; 0 when no coupon was used.
    /// </summary>
    public decimal CouponDiscount { get; private set; }

    /// <summary>
    /// Tax amount applied to the invoice.
    /// </summary>
    public decimal Tax { get; private set; }

    /// <summary>
    /// Final payable amount after discounts and tax.
    /// </summary>
    public decimal FinalAmount { get; private set; }

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

    /// <summary>
    /// The payment status reflected by the invoice (Paid, Refunded, ...).
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; }

    #endregion

    #region Relations

    private readonly List<InvoiceItemEntity> _items = new();

    /// <summary>
    /// The immutable line items of the invoice.
    /// </summary>
    public IReadOnlyCollection<InvoiceItemEntity> Items => _items.AsReadOnly();

    /// <summary>
    /// The parent order.
    /// </summary>
    public OrderEntity Order { get; private set; } = null!;

    /// <summary>
    /// The invoiced customer.
    /// </summary>
    public User User { get; private set; } = null!;

    #endregion
}
