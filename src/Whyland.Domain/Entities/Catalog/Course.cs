namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A purchasable or free educational product (s7.2).
/// The pricing model derives free status from a nullable price; no IsFree column exists (s7.3).
/// </summary>
public class Course : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="IntroductionVideo"/>; also the database column size.</summary>
    public const int MaxIntroductionVideoLength = 512;

    /// <summary>Maximum allowed length of <see cref="LatinName"/>; also the database column size.</summary>
    public const int MaxLatinNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="MainImage"/>; also the database column size.</summary>
    public const int MaxMainImageLength = 512;

    /// <summary>Maximum allowed length of <see cref="Name"/>; also the database column size.</summary>
    public const int MaxNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="ShortDescription"/>; also the database column size.</summary>
    public const int MaxShortDescriptionLength = 500;

    /// <summary>Maximum allowed length of <see cref="Slug"/>; also the database column size.</summary>
    public const int MaxSlugLength = 300;

    #endregion
    /// <summary>
    /// The course display name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Latin equivalent of the name, used to generate URL-friendly slugs.
    /// </summary>
    public string LatinName { get; private set; } = string.Empty;

    /// <summary>
    /// Short excerpt shown in listings before the full description.
    /// </summary>
    public string ShortDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Full description supporting HTML-editor content. Must be sanitized against an
    /// explicit allowlist before public display (s7.2.2).
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Unique, URL-safe and stable slug used for public course URLs (s7.2.3).
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Path or URL of the main cover image shown in listings.
    /// </summary>
    public string? MainImage { get; private set; }

    /// <summary>
    /// Null means the course is free; a non-null value greater than zero means it is paid (s7.3).
    /// A decimal-compatible money type is mandatory; float/double are forbidden (s7.2.4).
    /// </summary>
    public decimal? Price { get; private set; }

    /// <summary>
    /// The kind of discount applied to the course (s7.2.5).
    /// </summary>
    public DiscountType DiscountType { get; private set; } = DiscountType.None;

    /// <summary>
    /// Required when the discount is active. Percentages: 0..100;
    /// fixed amounts: must not make the final price negative (s7.2.5).
    /// </summary>
    public decimal? DiscountValue { get; private set; }

    /// <summary>
    /// Controls whether the course can currently be purchased; independent of display status (s7.2.6).
    /// </summary>
    public bool IsSaleActive { get; private set; }

    /// <summary>
    /// Public visibility status of the course (s7.2.7). Only published courses are discoverable.
    /// </summary>
    public DisplayStatus DisplayStatus { get; private set; } = DisplayStatus.Draft;

    /// <summary>
    /// Featured courses are promoted in the site's highlighted sections.
    /// </summary>
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// Amazing (special-offer) courses appear in the site's promotional deals section.
    /// </summary>
    public bool IsAmazing { get; private set; }

    /// <summary>
    /// Reference to the course introduction video shown on the landing page.
    /// </summary>
    public string? IntroductionVideo { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public Guid? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// The categories the course belongs to (s7.4).
    /// </summary>
    public ICollection<CourseCategory> CourseCategories { get; } = [];

    /// <summary>
    /// The instructors assigned to this course (s7.5).
    /// </summary>
    public ICollection<CourseInstructor> CourseInstructors { get; } = [];

    /// <summary>
    /// The logical sections grouping the course episodes (s7.6).
    /// </summary>
    public ICollection<CourseSection> Sections { get; } = [];

    /// <summary>
    /// FAQ items attached to this course (s7.10).
    /// </summary>
    public ICollection<CourseFaq> CourseFaqs { get; } = [];

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
