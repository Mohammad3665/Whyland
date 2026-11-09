namespace Whyland.Application.Features.Admin.Categories.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteCategoryCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for the category identifier.
    /// </summary>
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
