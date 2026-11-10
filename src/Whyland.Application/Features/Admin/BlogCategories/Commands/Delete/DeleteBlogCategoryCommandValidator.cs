namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteBlogCategoryCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteBlogCategoryCommandValidator : AbstractValidator<DeleteBlogCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for the category identifier.
    /// </summary>
    public DeleteBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}