namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>
/// Validates <see cref="CreateCourseSectionCommand"/> fields against the section domain
/// constraints.
/// </summary>
public sealed class CreateCourseSectionCommandValidator : AbstractValidator<CreateCourseSectionCommand>
{
    /// <summary>
    /// Configures the validation rules for every new section field.
    /// </summary>
    public CreateCourseSectionCommandValidator()
    {
        RuleFor(x => x.CourseId.Value)
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
