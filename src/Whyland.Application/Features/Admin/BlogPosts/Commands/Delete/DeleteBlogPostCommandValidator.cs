namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteBlogPostCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteBlogPostCommandValidator : AbstractValidator<DeleteBlogPostCommand>
{
    /// <summary>Configures the validation rules for the post identifier.</summary>
    public DeleteBlogPostCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
