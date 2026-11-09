namespace Whyland.Application.Features.Admin.Courses.Commands.Update;

/// <summary>
/// Command that updates an existing course: every editable value, its category and instructor
/// links, and optionally its cover image.
/// </summary>
/// <param name="Id">The identifier of the course to update.</param>
/// <param name="Name">The course display name.</param>
/// <param name="LatinName">The Latin name used to generate the slug.</param>
/// <param name="ShortDescription">The excerpt shown in listings.</param>
/// <param name="Description">The full (sanitized) description.</param>
/// <param name="Slug">The unique URL-safe slug.</param>
/// <param name="Price">The price in Toman; null means the course is free.</param>
/// <param name="DiscountType">The kind of discount applied to the course.</param>
/// <param name="DiscountValue">The discount amount, when a discount is active.</param>
/// <param name="IsSaleActive">Whether the course can currently be purchased.</param>
/// <param name="DisplayStatus">The public visibility status.</param>
/// <param name="IsFeatured">Whether the course is promoted as featured.</param>
/// <param name="IsAmazing">Whether the course is in the promotional deals section.</param>
/// <param name="CategoryIds">The categories the course belongs to.</param>
/// <param name="InstructorIds">The instructors assigned to the course.</param>
/// <param name="RemoveImage">Whether the current cover image should be cleared.</param>
/// <param name="Image">A replacement cover image upload, when one was selected.</param>
public sealed record UpdateCourseCommand(
    CourseId Id,
    string Name,
    string LatinName,
    string ShortDescription,
    string? Description,
    string Slug,
    decimal? Price,
    DiscountType DiscountType,
    decimal? DiscountValue,
    bool IsSaleActive,
    DisplayStatus DisplayStatus,
    bool IsFeatured,
    bool IsAmazing,
    IReadOnlyList<CategoryId> CategoryIds,
    IReadOnlyList<InstructorProfileId> InstructorIds,
    bool RemoveImage,
    CourseImageUpload? Image) : ICommand;
