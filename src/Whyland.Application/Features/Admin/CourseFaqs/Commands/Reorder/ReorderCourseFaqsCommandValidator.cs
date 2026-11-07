namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>Validates the course and the FAQ ids sent by drag &amp; drop.</summary>
public sealed class ReorderCourseFaqsCommandValidator : AbstractValidator<ReorderCourseFaqsCommand>
{
    public ReorderCourseFaqsCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .WhylandGreaterThan(0)
            .WithName("شناسه دوره");

        RuleFor(x => x.OrderedIds)
            .NotNull().WithMessage("ترتیب سوالات ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب سوالات خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه سوال ارسال‌شده نامعتبر است.");
    }
}
