namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>Validates <see cref="CreateCourseSectionCommand"/> against the section limits.</summary>
public sealed class CreateCourseSectionCommandValidator : AbstractValidator<CreateCourseSectionCommand>
{
    public CreateCourseSectionCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .WhylandGreaterThan(0)
            .WithName("شناسه دوره");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(CourseSection.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}
