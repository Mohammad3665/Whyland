namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>
/// Command that creates a new section inside one course and returns the identifier of the created
/// section.
/// </summary>
/// <param name="CourseId">The course the section belongs to.</param>
/// <param name="Title">The section title.</param>
/// <param name="DisplayOrder">The display order used for sorting sections.</param>
public sealed record CreateCourseSectionCommand(
    long CourseId,
    string Title,
    int DisplayOrder) : ICommand<long>;
