namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>Validates the section identifier before deletion.</summary>
public sealed class DeleteCourseSectionCommandValidator : AbstractValidator<DeleteCourseSectionCommand>
{
    public DeleteCourseSectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
