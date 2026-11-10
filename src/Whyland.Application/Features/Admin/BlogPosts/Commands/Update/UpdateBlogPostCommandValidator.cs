namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateBlogPostCommand"/> fields against the blog post domain constraints.
/// </summary>
public sealed class UpdateBlogPostCommandValidator : AbstractValidator<UpdateBlogPostCommand>
{
    /// <summary>Configures the validation rules for every updatable blog post field.</summary>
    public UpdateBlogPostCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(BlogPost.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.ShortDescription)
            .WhylandRequired()
            .WhylandMaxLen(BlogPost.MaxShortDescriptionLength)
            .WithName("توضیح کوتاه");

        RuleFor(x => x.Content)
            .WhylandRequired()
            .WithName("محتوای کامل");

        RuleFor(x => x.CategoryId.Value)
            .WhylandGreaterThan(0)
            .WithName("دسته‌بندی");

        RuleFor(x => x.Slug)
            .WhylandRequired()
            .WhylandMaxLen(BlogPost.MaxSlugLength)
            .WithName("اسلاگ");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");

        RuleFor(x => x.DisplayStatus)
            .IsInEnum()
            .WithName("وضعیت نمایش");
    }
}
