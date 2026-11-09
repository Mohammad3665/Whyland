namespace Whyland.Application.Features.Admin.Episodes.Commands.Reorder;

/// <summary>
/// Validates <see cref="ReorderEpisodesCommand"/> before executing the reorder.
/// </summary>
public sealed class ReorderEpisodesCommandValidator : AbstractValidator<ReorderEpisodesCommand>
{
    /// <summary>
    /// Configures the validation rules for the section and the episode ids sent by drag &amp; drop.
    /// </summary>
    public ReorderEpisodesCommandValidator()
    {
        RuleFor(x => x.SectionId.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه سرفصل");

        RuleFor(x => x.OrderedIds.Select(id => id.Value))
            .NotNull().WithMessage("ترتیب اپیزودها ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب اپیزودها خالی ارسال شده است.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("شناسه اپیزود ارسال‌شده نامعتبر است.");
    }
}
