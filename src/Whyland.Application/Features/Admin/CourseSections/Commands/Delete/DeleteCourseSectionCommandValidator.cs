namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteCourseSectionCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteCourseSectionCommandValidator : AbstractValidator<DeleteCourseSectionCommand>
{
    /// <summary>
    /// Configures the validation rule for the section identifier.
    /// </summary>
    public DeleteCourseSectionCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
