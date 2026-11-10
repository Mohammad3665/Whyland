namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Reorder;

/// <summary>
/// Validates <see cref="ReorderBlogCategoriesCommand"/> before executing the reorder.
/// </summary>
public sealed class ReorderBlogCategoriesCommandValidator : AbstractValidator<ReorderBlogCategoriesCommand>
{
    /// <summary>
    /// Configures the validation rules for the posted ordering sequence.
    /// </summary>
    public ReorderBlogCategoriesCommandValidator()
    {
        RuleFor(x => x.OrderedIds.Select(id => id.Value))
            .NotNull().WithMessage("ترتیب دسته‌بندی‌ها ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب دسته‌بندی‌ها خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه دسته‌بندی ارسال‌شده نامعتبر است.");
    }
}