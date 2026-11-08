namespace Whyland.Shared.Dtos.Catalog.CourseSection;

/// <summary>
/// Read model representing one section of a course's curriculum in the admin list.
/// </summary>
/// <param name="Id">The unique identifier of the section.</param>
/// <param name="CourseId">The course the section belongs to.</param>
/// <param name="Title">The section title.</param>
/// <param name="DisplayOrder">Manual ordering weight; lower values appear first.</param>
/// <param name="EpisodeCount">How many live episodes the section currently has.</param>
/// <param name="CreatedAt">The UTC timestamp when the section was created.</param>
public sealed record CourseSectionListItemDto(
    long Id,
    long CourseId,
    string Title,
    int DisplayOrder,
    int EpisodeCount,
    DateTime CreatedAt);
