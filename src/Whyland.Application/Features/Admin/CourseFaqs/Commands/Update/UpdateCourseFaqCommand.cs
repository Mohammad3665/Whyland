namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;

/// <summary>Updates the question, answer and order of a course FAQ.</summary>
public sealed record UpdateCourseFaqCommand(
    long Id,
    string Title,
    string Description,
    int DisplayOrder) : ICommand;
