namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;

/// <summary>
/// Command that updates an existing course FAQ with a new question, answer and display order.
/// </summary>
/// <param name="Id">The identifier of the question to update.</param>
/// <param name="Title">The new question text.</param>
/// <param name="Description">The new answer text.</param>
/// <param name="DisplayOrder">The new display order used for sorting the course's questions.</param>
/// <param name="IsActive">Whether the question is visible on the public course page.</param>
public sealed record UpdateCourseFaqCommand(
    CourseFaqId Id,
    string Title,
    string Description,
    int DisplayOrder,
    bool IsActive) : ICommand;
