namespace Whyland.Application.Features.Admin.Categories.Commands.Create;

/// <summary>
/// Validates <see cref="CreateCategoryCommand"/> fields against the category domain constraints.
/// </summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for every new category field.
    /// </summary>
    public CreateCategoryCommandValidator()
    {
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
