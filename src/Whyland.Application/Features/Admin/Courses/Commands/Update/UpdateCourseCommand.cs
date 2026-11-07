using Whyland.Domain.Common.Enums;

namespace Whyland.Application.Features.Admin.Courses.Commands.Update;

/// <summary>
/// Command that updates an existing course: every editable value, its category and instructor
/// links, and optionally its cover image.
/// </summary>
public sealed record UpdateCourseCommand(
    long Id,
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
    IReadOnlyList<long> CategoryIds,
    IReadOnlyList<Guid> InstructorIds,
    bool RemoveImage,
    CourseImageUpload? Image) : ICommand;
