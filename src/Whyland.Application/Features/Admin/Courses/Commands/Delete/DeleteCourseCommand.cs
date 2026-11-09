namespace Whyland.Application.Features.Admin.Courses.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing course by its identifier.
/// </summary>
/// <param name="Id">The identifier of the course to soft-delete.</param>
public sealed record DeleteCourseCommand(CourseId Id) : ICommand;
