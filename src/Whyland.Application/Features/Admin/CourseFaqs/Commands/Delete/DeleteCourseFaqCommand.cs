namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>Soft-deletes a course FAQ.</summary>
public sealed record DeleteCourseFaqCommand(long Id) : ICommand;
