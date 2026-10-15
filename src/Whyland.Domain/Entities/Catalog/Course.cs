
namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A purchasable or free educational product (s7.2).
/// The pricing model derives free status from a nullable price; no IsFree column exists (s7.3).
/// </summary>
public class Course : BaseEntity<long>
{
    public string Name { get; set; } = string.Empty;

    public string LatinName { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Full description supporting HTML-editor content. Must be sanitized against an
    /// explicit allowlist before public display (s7.2.2).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique, URL-safe and stable slug used for public course URLs (s7.2.3).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    public string? MainImage { get; set; }

    /// <summary>
    /// Null means the course is free; a non-null value greater than zero means it is paid (s7.3).
    /// A decimal-compatible money type is mandatory; float/double are forbidden (s7.2.4).
    /// </summary>
    public decimal? Price { get; private set; }

    public DiscountType DiscountType { get; set; } = DiscountType.None;

    /// <summary>
    /// Required when the discount is active. Percentages: 0..100;
    /// fixed amounts: must not make the final price negative (s7.2.5).
    /// </summary>
    public decimal? DiscountValue { get; set; }

    /// <summary>
    /// Controls whether the course can currently be purchased; independent of display status (s7.2.6).
    /// </summary>
    public bool IsSaleActive { get; set; }

    public DisplayStatus DisplayStatus { get; set; } = DisplayStatus.Draft;

    public bool IsFeatured { get; set; }

    public bool IsAmazing { get; set; }

    public string? IntroductionVideo { get; set; }

    public ICollection<CourseCategory> CourseCategories { get; set; } = [];

    public ICollection<CourseInstructor> CourseInstructors { get; set; } = [];

    public ICollection<CourseSection> Sections { get; set; } = [];

    public ICollection<CourseFaq> CourseFaqs { get; set; } = [];

    /// <summary>
    /// Domain-derived free status. Captured at run time only; never stored (s7.3.2).
    /// </summary>
    public bool IsFree => Price is null;

    /// <summary>
    /// Domain-derived paid status.
    /// </summary>
    public bool IsPaid => Price is not null;

    /// <summary>
    /// Sets the course price. Null makes the course free; a value must be greater than zero (s7.3.3).
    /// </summary>
    public void SetPrice(decimal? price)
    {
        if (price is not null && price <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price),
                "A paid product price must be greater than zero; null represents a free product (s7.3.3).");

        Price = price;
    }

    /// <summary>
    /// Computes the effective unit price after the course-level discount (s7.2.5).
    /// The final payable price must be computed centrally in the Domain/Application layer.
    /// </summary>
    public decimal GetEffectivePrice()
    {
        if (IsFree) return 0m;

        var price = Price!.Value;

        if (DiscountType is DiscountType.None) return price;

        return DiscountType switch
        {
            DiscountType.Percentage when DiscountValue is >= 0 and <= 100 =>
                price - decimal.Round(price * DiscountValue!.Value / 100m, 2),
            DiscountType.FixedAmount when DiscountValue is not null =>
                Math.Max(0m, price - DiscountValue.Value),
            _ => price
        };
    }
}
