namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Reorder;

/// <summary>
/// Validates the payload produced by dragging article rows.
/// </summary>
public sealed class ReorderBlogPostsCommandValidator : AbstractValidator<ReorderBlogPostsCommand>
{
    /// <summary>Configures the rules for the reorder payload.</summary>
    public ReorderBlogPostsCommandValidator()
    {
        RuleFor(x => x.Ordering)
            .NotNull().WithMessage("ترتیب مقالات ارسال نشده است.")
            .NotEmpty().WithMessage("ترتیب مقالات خالی ارسال شده است.");

        RuleFor(x => x.Ordering)
            .Must(items => items.All(item => item.Id.Value > 0 && item.DisplayOrder > 0))
            .WithMessage("شناسه یا ترتیب مقاله ارسال‌شده نامعتبر است.");

        RuleFor(x => x.Ordering)
            .Must(items => items.Select(item => item.Id.Value).Distinct().Count() == items.Count)
            .WithMessage("شناسه مقاله تکراری در ترتیب ارسال شده است.");

        RuleFor(x => x.Ordering)
            .Must(items => items.Select(item => item.DisplayOrder).Distinct().Count() == items.Count)
            .WithMessage("ترتیب نمایش تکراری در ترتیب ارسال شده است.");
    }
}
