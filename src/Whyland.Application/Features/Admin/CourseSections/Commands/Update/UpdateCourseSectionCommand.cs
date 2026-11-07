namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>Updates the title and display order of a course section.</summary>
public sealed record UpdateCourseSectionCommand(long Id, string Title, int DisplayOrder) : ICommand;
