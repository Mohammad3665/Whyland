namespace Whyland.Shared.Dtos.Catalog.Course;

/// <summary>
/// One row of the admin course table. The listing carries everything the table and its badges
/// show, so opening a row costs no extra round-trip; the edit form reads the richer details DTO
/// through its own endpoint.
/// </summary>
/// <param name="Id">The unique identifier of the course.</param>
/// <param name="Name">The Persian/course display name.</param>
/// <param name="LatinName">The Latin name used to build the slug.</param>
/// <param name="Slug">The unique URL-safe slug.</param>
/// <param name="MainImageUrl">Public URL of the cover image, or null when no image was uploaded.</param>
/// <param name="ThumbnailUrl">Public URL of its thumbnail, or null.</param>
/// <param name="Price">The price in Toman; null means the course is free.</param>
/// <param name="DiscountType">The discount kind as a string (None, Percentage, FixedAmount).</param>
/// <param name="DiscountValue">The discount amount, when one is set.</param>
/// <param name="IsSaleActive">Whether the course can currently be purchased.</param>
/// <param name="DisplayStatus">The visibility status as a string (Draft, Published, Hidden).</param>
/// <param name="IsFeatured">Whether the course is promoted as featured.</param>
/// <param name="IsAmazing">Whether the course is in the promotional deals section.</param>
/// <param name="CategoryNames">The joined category names, ready to show in one cell.</param>
/// <param name="InstructorNames">The joined instructor names, ready to show in one cell.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identifier of the creating account.</param>
/// <param name="CreatedByName">Full name of the creating account, when it resolved.</param>
/// <param name="UpdatedAt">UTC timestamp of the last change.</param>
/// <param name="UpdatedBy">Identifier of the last editing account.</param>
/// <param name="UpdatedByName">Full name of the last editing account, when it resolved.</param>
public sealed record CourseListItemDto(
    long Id,
    string Name,
    string LatinName,
    string Slug,
    string? MainImageUrl,
    string? ThumbnailUrl,
    decimal? Price,
    string DiscountType,
    decimal? DiscountValue,
    bool IsSaleActive,
    string DisplayStatus,
    bool IsFeatured,
    bool IsAmazing,
    string? CategoryNames,
    string? InstructorNames,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTime UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName);
