namespace Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;

/// <summary>Validates the course and the section ids sent by drag &amp; drop.</summary>
public sealed class ReorderCourseSectionsCommandValidator : AbstractValidator<ReorderCourseSectionsCommand>
{
    public ReorderCourseSectionsCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .WhylandGreaterThan(0)
            .WithName("شناسه دوره");

        RuleFor(x => x.OrderedIds)
            .NotNull().WithMessage("ترتیب سرفصل‌ها ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب سرفصل‌ها خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه سرفصل ارسال‌شده نامعتبر است.");
    }
}
