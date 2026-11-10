namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateBlogCategoryCommand"/> fields against the blog category domain constraints.
/// </summary>
public sealed class UpdateBlogCategoryCommandValidator : AbstractValidator<UpdateBlogCategoryCommand>
{
    /// <summary>
    /// Configures the validation rules for every updatable blog category field.
    /// </summary>
    public UpdateBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

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