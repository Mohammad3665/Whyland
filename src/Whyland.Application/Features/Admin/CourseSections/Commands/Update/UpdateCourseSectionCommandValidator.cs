namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateCourseSectionCommand"/> fields against the section domain
/// constraints.
/// </summary>
public sealed class UpdateCourseSectionCommandValidator : AbstractValidator<UpdateCourseSectionCommand>
{
    /// <summary>
    /// Configures the validation rules for every updatable section field.
    /// </summary>
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
