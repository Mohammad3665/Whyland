namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>Validates <see cref="UpdateCourseSectionCommand"/> against the section limits.</summary>
public sealed class UpdateCourseSectionCommandValidator : AbstractValidator<UpdateCourseSectionCommand>
{
    public UpdateCourseSectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(CourseSection.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}
