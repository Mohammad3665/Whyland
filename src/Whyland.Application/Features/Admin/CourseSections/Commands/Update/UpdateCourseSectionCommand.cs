namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>
/// Command that updates an existing section with a new title and display order.
/// </summary>
/// <param name="Id">The identifier of the section to update.</param>
/// <param name="Title">The new section title.</param>
/// <param name="DisplayOrder">The new display order used for sorting sections.</param>
public sealed record UpdateCourseSectionCommand(long Id, string Title, int DisplayOrder) : ICommand;
