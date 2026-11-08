namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>
/// Validates <see cref="CreateCourseFaqCommand"/> fields against the course FAQ domain
/// constraints.
/// </summary>
public sealed class CreateCourseFaqCommandValidator : AbstractValidator<CreateCourseFaqCommand>
{
    /// <summary>
    /// Configures the validation rules for every new question field.
    /// </summary>
    public CreateCourseFaqCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .WhylandGreaterThan(0)
            .WithName("شناسه دوره");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(CourseFaq.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.Description)
            .WhylandRequired()
            .WhylandMaxLen(CourseFaq.MaxDescriptionLength)
            .WithName("توضیحات");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}
