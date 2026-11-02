namespace Whyland.Application.Features.Admin.Categories.Commands.Reorder;

/// <summary>
/// Validates <see cref="ReorderCategoriesCommand"/> before executing the reorder.
/// </summary>
public sealed class ReorderCategoriesCommandValidator : AbstractValidator<ReorderCategoriesCommand>
{
    /// <summary>
    /// Configures the validation rules for the posted ordering sequence.
    /// </summary>
    public ReorderCategoriesCommandValidator()
    {
        RuleFor(x => x.OrderedIds)
            .NotNull().WithMessage("ترتیب دسته‌بندی‌ها ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب دسته‌بندی‌ها خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه دسته‌بندی ارسال‌شده نامعتبر است.");
    }
}
