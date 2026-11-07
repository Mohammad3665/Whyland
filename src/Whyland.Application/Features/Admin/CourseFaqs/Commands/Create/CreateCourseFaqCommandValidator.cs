namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>Validates <see cref="CreateCourseFaqCommand"/> against the course FAQ limits.</summary>
public sealed class CreateCourseFaqCommandValidator : AbstractValidator<CreateCourseFaqCommand>
{
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
