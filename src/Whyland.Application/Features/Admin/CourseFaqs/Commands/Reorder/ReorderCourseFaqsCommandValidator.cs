namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>
/// Validates <see cref="ReorderCourseFaqsCommand"/> before executing the reorder.
/// </summary>
public sealed class ReorderCourseFaqsCommandValidator : AbstractValidator<ReorderCourseFaqsCommand>
{
    /// <summary>
    /// Configures the validation rules for the course and the question ids sent by drag &amp; drop.
    /// </summary>
    public ReorderCourseFaqsCommandValidator()
    {
        RuleFor(x => x.CourseId.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه دوره");

        RuleFor(x => x.OrderedIds.Select(id => id.Value))
            .NotNull().WithMessage("ترتیب سوالات ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب سوالات خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه سوال ارسال‌شده نامعتبر است.");
    }
}
