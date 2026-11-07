namespace Whyland.Shared.Dtos.Catalog.Course;

/// <summary>
/// Everything the course edit form needs: the stored values plus the selected category and
/// instructor ids, and the public URLs built from the stored image path.
/// </summary>
/// <param name="Id">The unique identifier of the course.</param>
/// <param name="Name">The Persian/course display name.</param>
/// <param name="LatinName">The Latin name used to build the slug.</param>
/// <param name="ShortDescription">The excerpt shown in listings.</param>
/// <param name="Description">The full HTML description.</param>
/// <param name="Slug">The unique URL-safe slug.</param>
/// <param name="MainImageUrl">Public URL of the cover image, or null.</param>
/// <param name="ThumbnailUrl">Public URL of the cover thumbnail, or null.</param>
/// <param name="Price">The price in Toman; null means free.</param>
/// <param name="DiscountType">The discount kind as a string (None, Percentage, FixedAmount).</param>
/// <param name="DiscountValue">The discount amount, when one is set.</param>
/// <param name="IsSaleActive">Whether the course can currently be purchased.</param>
/// <param name="DisplayStatus">The visibility status as a string (Draft, Published, Hidden).</param>
/// <param name="IsFeatured">Whether the course is promoted as featured.</param>
/// <param name="IsAmazing">Whether the course is in the promotional deals section.</param>
/// <param name="CategoryIds">The identifiers of the selected categories.</param>
/// <param name="InstructorIds">The identifiers of the selected instructor profiles.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identifier of the creating account.</param>
/// <param name="CreatedByName">Full name of the creating account, when it resolved.</param>
/// <param name="UpdatedAt">UTC timestamp of the last change.</param>
/// <param name="UpdatedBy">Identifier of the last editing account.</param>
/// <param name="UpdatedByName">Full name of the last editing account, when it resolved.</param>
public sealed record CourseDetailsDto(
    long Id,
    string Name,
    string LatinName,
    string ShortDescription,
    string? Description,
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
    long[] CategoryIds,
    Guid[] InstructorIds,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTime UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName);
