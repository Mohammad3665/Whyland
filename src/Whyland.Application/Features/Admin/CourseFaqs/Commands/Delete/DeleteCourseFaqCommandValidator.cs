namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>Validates the course FAQ identifier before deletion.</summary>
public sealed class DeleteCourseFaqCommandValidator : AbstractValidator<DeleteCourseFaqCommand>
{
    public DeleteCourseFaqCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
