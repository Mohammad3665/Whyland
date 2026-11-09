namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing section by its identifier.
/// </summary>
/// <param name="Id">The identifier of the section to soft-delete.</param>
public sealed record DeleteCourseSectionCommand(CourseSectionId Id) : ICommand;
