namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>
/// Command that creates a new FAQ item inside one course and returns the identifier of the created
/// question.
/// </summary>
/// <param name="CourseId">The course the question belongs to.</param>
/// <param name="Title">The question text.</param>
/// <param name="Description">The answer text.</param>
/// <param name="DisplayOrder">The display order used for sorting the course's questions.</param>
/// <param name="IsActive">Whether the question is visible on the public course page.</param>
public sealed record CreateCourseFaqCommand(
    CourseId CourseId,
    string Title,
    string Description,
    int DisplayOrder,
    bool IsActive) : ICommand<CourseFaqId>;
