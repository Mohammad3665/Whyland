namespace Whyland.Application.Features.Admin.Courses.Commands.Delete;

/// <summary>Validates <see cref="DeleteCourseCommand"/> before deletion.</summary>
public sealed class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
{
    /// <summary>Configures the validation rule for the course identifier.</summary>
    public DeleteCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
