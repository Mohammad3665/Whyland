namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing course FAQ by its identifier.
/// </summary>
/// <param name="Id">The identifier of the question to soft-delete.</param>
public sealed record DeleteCourseFaqCommand(long Id) : ICommand;
