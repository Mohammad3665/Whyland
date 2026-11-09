namespace Whyland.Shared.Dtos.Catalog.CourseFaq;

/// <summary>
/// Read model representing one question shown under the FAQ section of a course.
/// </summary>
/// <param name="Id">The unique identifier of the question.</param>
/// <param name="CourseId">The course the question belongs to.</param>
/// <param name="Title">The question text.</param>
/// <param name="Description">The answer text.</param>
/// <param name="DisplayOrder">Manual ordering weight; lower values appear first.</param>
/// <param name="IsActive">Whether the question is visible on the public course page.</param>
/// <param name="CreatedAt">The UTC timestamp when the question was created.</param>
public sealed record CourseFaqListItemDto(
    long Id,
    long CourseId,
    string Title,
    string Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt);
