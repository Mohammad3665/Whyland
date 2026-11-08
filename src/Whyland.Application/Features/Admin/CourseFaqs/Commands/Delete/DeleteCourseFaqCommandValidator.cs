namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteCourseFaqCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteCourseFaqCommandValidator : AbstractValidator<DeleteCourseFaqCommand>
{
    /// <summary>
    /// Configures the validation rule for the question identifier.
    /// </summary>
    public DeleteCourseFaqCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
