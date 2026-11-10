namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Create;

/// <summary>
/// Validates <see cref="CreateBlogPostCommand"/> fields against the blog post domain constraints.
/// </summary>
public sealed class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    /// <summary>Configures the validation rules for every new blog post field.</summary>
    public CreateBlogPostCommandValidator()
    {
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

        RuleFor(x => x.AuthorId.Value)
            .NotEmpty()
            .WithName("نویسنده");

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
