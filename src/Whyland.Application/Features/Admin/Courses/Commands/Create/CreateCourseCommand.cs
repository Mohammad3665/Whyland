using Whyland.Domain.Common.Enums;

namespace Whyland.Application.Features.Admin.Courses.Commands.Create;

/// <summary>
/// Command that creates a new course and returns the identifier of the created row.
/// A cover image is optional; when present it is stored before the row is saved.
/// </summary>
public sealed record CreateCourseCommand(
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
    CourseImageUpload? Image) : ICommand<long>;
