namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;

/// <summary>Validates <see cref="UpdateCourseFaqCommand"/> against the course FAQ limits.</summary>
public sealed class UpdateCourseFaqCommandValidator : AbstractValidator<UpdateCourseFaqCommand>
{
    public UpdateCourseFaqCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

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
