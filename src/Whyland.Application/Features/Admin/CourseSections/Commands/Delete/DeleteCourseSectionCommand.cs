namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>Soft-deletes a course section.</summary>
public sealed record DeleteCourseSectionCommand(long Id) : ICommand;
