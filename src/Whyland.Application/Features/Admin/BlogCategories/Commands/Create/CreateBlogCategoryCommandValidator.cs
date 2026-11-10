namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Create;

/// <summary>
/// Validates <see cref="CreateBlogCategoryCommand"/> fields against the blog category domain constraints.
/// </summary>
public sealed class CreateBlogCategoryCommandValidator : AbstractValidator<CreateBlogCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for every new blog category field.
    /// </summary>
    public CreateBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .WhylandRequired()
            .WhylandMaxLen(BlogCategory.MaxNameLength)
            .WithName("نام");

        RuleFor(x => x.LatinName)
            .WhylandRequired()
            .WhylandMaxLen(BlogCategory.MaxLatinNameLength)
            .WithName("نام لاتین");

        RuleFor(x => x.Slug)
            .WhylandRequired()
            .WhylandMaxLen(BlogCategory.MaxSlugLength)
            .WithName("اسلاگ");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}