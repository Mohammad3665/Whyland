using System.Text.RegularExpressions;

using CourseCategoryEntity = Whyland.Domain.Model.Catalog.CourseCategory.Entities.CourseCategory;
using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;
using CourseInstructorEntity = Whyland.Domain.Model.Catalog.CourseInstructor.Entities.CourseInstructor;
using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;
using Whyland.Domain.Model.Catalog.Course.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.Course.Entities;

/// <summary>
/// A purchasable or free educational product (s7.2).
/// The pricing model derives free status from a nullable price; no IsFree column exists (s7.3).
/// </summary>
public class Course : BaseEntity<CourseId>, ISoftDeletable
{
    #region Properties

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
    /// Relative path of the main cover image under the image storage root.
    /// The public URL is built by the read models, so moving the storage never rewrites a row.
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
    /// The categories the course belongs to (s7.4).
    /// </summary>
    public ICollection<CourseCategoryEntity> CourseCategories { get; } = [];

    /// <summary>
    /// The instructors assigned to this course (s7.5).
    /// </summary>
    public ICollection<CourseInstructorEntity> CourseInstructors { get; } = [];

    /// <summary>
    /// The logical sections grouping the course episodes (s7.6).
    /// </summary>
    public ICollection<CourseSectionEntity> Sections { get; } = [];

    /// <summary>
    /// FAQ items attached to this course (s7.10).
    /// </summary>
    public ICollection<CourseFaqEntity> CourseFaqs { get; } = [];

    /// <summary>
    /// Domain-derived free status. Captured at run time only; never stored (s7.3.2).
    /// </summary>
    public bool IsFree => Price is null;

    /// <summary>
    /// Domain-derived paid status.
    /// </summary>
    public bool IsPaid => Price is not null;

    #endregion

    #region Construction

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

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

    /// <summary>Slug shape: lower-case letters and digits separated by single hyphens.</summary>
    private static readonly Regex SlugPattern = new(
        @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private Course() { }

    /// <summary>
    /// Creates a new course after trimming and validating the inputs.
    /// Relations and the uploaded image are attached by the caller once the row exists.
    /// </summary>
    /// <param name="name">The course display name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name. Will be trimmed.</param>
    /// <param name="shortDescription">The excerpt shown in listings. Will be trimmed.</param>
    /// <param name="description">The optional full description. Will be trimmed.</param>
    /// <param name="slug">The URL-safe slug. Will be trimmed and lower-cased.</param>
    /// <param name="price">The price in Toman; null means the course is free.</param>
    /// <param name="discountType">The kind of discount applied to the course.</param>
    /// <param name="discountValue">The discount amount, when a discount is active.</param>
    /// <param name="isSaleActive">Whether the course can currently be purchased.</param>
    /// <param name="displayStatus">The public visibility status.</param>
    /// <param name="isFeatured">Whether the course is promoted as featured.</param>
    /// <param name="isAmazing">Whether the course is in the promotional deals section.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new course,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Course> Create(
        string name,
        string latinName,
        string shortDescription,
        string? description,
        string slug,
        decimal? price,
        DiscountType discountType,
        decimal? discountValue,
        bool isSaleActive,
        DisplayStatus displayStatus,
        bool isFeatured,
        bool isAmazing)
    {
        var course = new Course();

        var applied = course.Apply(
            name,
            latinName,
            shortDescription,
            description,
            slug,
            price,
            discountType,
            discountValue,
            isSaleActive,
            displayStatus,
            isFeatured,
            isAmazing);

        return applied.IsFailure ? applied.Error : course;
    }

    /// <summary>
    /// Updates the editable values of an existing course, leaving the cover image alone:
    /// the image is its own intent because it is stored before this row is saved.
    /// </summary>
    /// <param name="name">The new course display name. Will be trimmed.</param>
    /// <param name="latinName">The new Latin name. Will be trimmed.</param>
    /// <param name="shortDescription">The new excerpt. Will be trimmed.</param>
    /// <param name="description">The new optional full description. Will be trimmed.</param>
    /// <param name="slug">The new URL-safe slug. Will be trimmed and lower-cased.</param>
    /// <param name="price">The new price in Toman; null means the course is free.</param>
    /// <param name="discountType">The new kind of discount.</param>
    /// <param name="discountValue">The new discount amount, when a discount is active.</param>
    /// <param name="isSaleActive">Whether the course can currently be purchased.</param>
    /// <param name="displayStatus">The new public visibility status.</param>
    /// <param name="isFeatured">Whether the course is promoted as featured.</param>
    /// <param name="isAmazing">Whether the course is in the promotional deals section.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(
        string name,
        string latinName,
        string shortDescription,
        string? description,
        string slug,
        decimal? price,
        DiscountType discountType,
        decimal? discountValue,
        bool isSaleActive,
        DisplayStatus displayStatus,
        bool isFeatured,
        bool isAmazing)
        => Apply(
            name,
            latinName,
            shortDescription,
            description,
            slug,
            price,
            discountType,
            discountValue,
            isSaleActive,
            displayStatus,
            isFeatured,
            isAmazing);

    /// <summary>
    /// Stores the relative path of the cover image, or clears it when <paramref name="mainImage"/>
    /// is null. The file itself is written and removed by the image storage service.
    /// </summary>
    /// <param name="mainImage">The relative path returned by the storage service, or null to clear it.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure when the path is too long.</returns>
    public Result SetMainImage(string? mainImage)
    {
        var trimmed = mainImage?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            MainImage = null;
            return Result.Success();
        }

        if (trimmed.Length > MaxMainImageLength)
            return CourseErrors.FieldTooLong(nameof(MainImage), MaxMainImageLength);

        MainImage = trimmed;
        return Result.Success();
    }

    /// <summary>
    /// Replaces the category links with exactly the requested set, keeping the rows that stay.
    /// </summary>
    /// <param name="categoryIds">The categories the course should belong to.</param>
    public void ReplaceCategories(IEnumerable<long> categoryIds)
    {
        var desired = categoryIds.Distinct().Select(CategoryId.From).ToHashSet();

        foreach (var link in CourseCategories.Where(x => !desired.Contains(x.CategoryId)).ToList())
            CourseCategories.Remove(link);

        var existing = CourseCategories.Select(x => x.CategoryId).ToHashSet();

        foreach (var id in desired.Where(id => !existing.Contains(id)))
            CourseCategories.Add(CourseCategoryEntity.Create(this, id));
    }

    /// <summary>
    /// Replaces the instructor links with exactly the requested set, keeping the rows that stay.
    /// </summary>
    /// <param name="instructorProfileIds">The instructor profiles assigned to the course.</param>
    public void ReplaceInstructors(IEnumerable<Guid> instructorProfileIds)
    {
        var desired = instructorProfileIds
            .Distinct()
            .Select(InstructorProfileId.From)
            .ToHashSet();

        foreach (var link in CourseInstructors.Where(x => !desired.Contains(x.InstructorProfileId)).ToList())
            CourseInstructors.Remove(link);

        var existing = CourseInstructors.Select(x => x.InstructorProfileId).ToHashSet();

        foreach (var id in desired.Where(id => !existing.Contains(id)))
            CourseInstructors.Add(CourseInstructorEntity.Create(this, id));
    }

    /// <summary>
    /// Marks the course as deleted so the global query filter hides it, keeping the row and its
    /// relationships. The first deletion time wins; re-deleting an already hidden row leaves its
    /// original stamp intact.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the price directly. Used by the seeding paths; the admin form goes through
    /// <see cref="Update"/> so the discount is validated against the new price.
    /// </summary>
    /// <param name="price">The new price; null means the course is free.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a paid price is not greater than zero.</exception>
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
    /// <returns>The payable price in Toman, never below zero.</returns>
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

    /// <summary>
    /// Trims, validates and assigns every editable value. Shared by create and update so both paths
    /// enforce exactly the same rules; on failure the instance is left unchanged.
    /// </summary>
    /// <param name="name">The course display name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name. Will be trimmed.</param>
    /// <param name="shortDescription">The excerpt shown in listings. Will be trimmed.</param>
    /// <param name="description">The optional full description. Will be trimmed.</param>
    /// <param name="slug">The URL-safe slug. Will be trimmed and lower-cased.</param>
    /// <param name="price">The price in Toman; null means the course is free.</param>
    /// <param name="discountType">The kind of discount applied to the course.</param>
    /// <param name="discountValue">The discount amount, when a discount is active.</param>
    /// <param name="isSaleActive">Whether the course can currently be purchased.</param>
    /// <param name="displayStatus">The public visibility status.</param>
    /// <param name="isFeatured">Whether the course is promoted as featured.</param>
    /// <param name="isAmazing">Whether the course is in the promotional deals section.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(
        string name,
        string latinName,
        string shortDescription,
        string? description,
        string slug,
        decimal? price,
        DiscountType discountType,
        decimal? discountValue,
        bool isSaleActive,
        DisplayStatus displayStatus,
        bool isFeatured,
        bool isAmazing)
    {
        var trimmedName = name?.Trim();
        var trimmedLatinName = latinName?.Trim();
        var trimmedShortDescription = shortDescription?.Trim() ?? string.Empty;
        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        var trimmedSlug = slug?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(trimmedName))
            return CourseErrors.FieldRequired(nameof(Name));

        if (trimmedName.Length > MaxNameLength)
            return CourseErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(trimmedLatinName))
            return CourseErrors.FieldRequired(nameof(LatinName));

        if (trimmedLatinName.Length > MaxLatinNameLength)
            return CourseErrors.FieldTooLong(nameof(LatinName), MaxLatinNameLength);

        if (trimmedShortDescription.Length > MaxShortDescriptionLength)
            return CourseErrors.FieldTooLong(nameof(ShortDescription), MaxShortDescriptionLength);

        if (string.IsNullOrWhiteSpace(trimmedSlug))
            return CourseErrors.FieldRequired(nameof(Slug));

        if (trimmedSlug.Length > MaxSlugLength)
            return CourseErrors.FieldTooLong(nameof(Slug), MaxSlugLength);

        if (!SlugPattern.IsMatch(trimmedSlug))
            return CourseErrors.InvalidSlug();

        if (price is <= 0m)
            return CourseErrors.InvalidPrice();

        if (!Enum.IsDefined(discountType))
            return CourseErrors.InvalidDiscount();

        if (!Enum.IsDefined(displayStatus))
            return CourseErrors.InvalidDisplayStatus();

        // A discount with no type is no discount, even if the client left a stale value behind.
        var normalizedDiscountValue = discountType is DiscountType.None ? null : discountValue;

        if (discountType is DiscountType.Percentage)
        {
            if (normalizedDiscountValue is null or < 0m or > 100m)
                return CourseErrors.InvalidDiscount();
        }
        else if (discountType is DiscountType.FixedAmount)
        {
            if (price is null)
                return CourseErrors.DiscountRequiresPrice();

            if (normalizedDiscountValue is null or < 0m || normalizedDiscountValue > price)
                return CourseErrors.InvalidDiscount();
        }

        Name = trimmedName;
        LatinName = trimmedLatinName;
        ShortDescription = trimmedShortDescription;
        Description = trimmedDescription;
        Slug = trimmedSlug;
        Price = price;
        DiscountType = discountType;
        DiscountValue = normalizedDiscountValue;
        IsSaleActive = isSaleActive;
        DisplayStatus = displayStatus;
        IsFeatured = isFeatured;
        IsAmazing = isAmazing;

        return Result.Success();
    }

    #endregion
}
