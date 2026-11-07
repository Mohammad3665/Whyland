namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>Creates a FAQ item inside one course.</summary>
public sealed record CreateCourseFaqCommand(
    long CourseId,
    string Title,
    string Description,
    int DisplayOrder) : ICommand<long>;
