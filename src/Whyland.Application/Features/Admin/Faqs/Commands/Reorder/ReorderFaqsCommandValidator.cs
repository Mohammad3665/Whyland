namespace Whyland.Application.Features.Admin.Faqs.Commands.Reorder;

/// <summary>
/// Validates <see cref="ReorderFaqsCommand"/> before executing the reorder.
/// </summary>
public sealed class ReorderFaqsCommandValidator : AbstractValidator<ReorderFaqsCommand>
{
    public ReorderFaqsCommandValidator()
    {
        RuleFor(x => x.OrderedIds)
            .NotNull().WithMessage("ترتیب سوالات ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب سوالات خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه سوال ارسال‌شده نامعتبر است.");
    }
}
