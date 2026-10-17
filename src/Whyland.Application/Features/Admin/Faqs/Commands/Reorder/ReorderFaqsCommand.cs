namespace Whyland.Application.Features.Admin.Faqs.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the FAQ aggregate (s4.12).
/// </summary>
/// <param name="OrderedIds">The FAQ identifiers in the desired visual order; index + 1 becomes DisplayOrder.</param>
public sealed record ReorderFaqsCommand(IReadOnlyList<long> OrderedIds) : ICommand;

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
