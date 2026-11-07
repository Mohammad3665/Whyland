namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>Creates a section inside one course.</summary>
public sealed record CreateCourseSectionCommand(
    long CourseId,
    string Title,
    int DisplayOrder) : ICommand<long>;
