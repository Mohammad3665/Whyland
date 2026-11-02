namespace Whyland.Application.Features.Admin.Categories.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateCategoryCommand"/> fields against the category domain constraints.
/// </summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for every updatable category field.
    /// </summary>
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Name)
            .WhylandRequired()
            .WhylandMaxLen(Category.MaxNameLength)
            .WithName("نام");

        RuleFor(x => x.LatinName)
            .WhylandRequired()
            .WhylandMaxLen(Category.MaxLatinNameLength)
            .WithName("نام لاتین");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}
