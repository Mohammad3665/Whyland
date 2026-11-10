using Whyland.Domain.Model.Blog.BlogPost.Errors;

namespace Whyland.Application.Features.Admin.BlogPosts;

/// <summary>
/// Checks the references a blog post save depends on: the category it is filed under and the author
/// it is attributed to. Both are rows in other aggregates, so they are resolved here once for create
/// and update instead of each handler spelling the same checks.
/// </summary>
internal static class BlogPostReferenceValidator
{
    /// <summary>
    /// Validates the category and author, or returns the first missing/inactive reference as the
    /// domain error the page shows.
    /// </summary>
    /// <param name="unitOfWork">The unit of work providing the reference repositories.</param>
    /// <param name="authorId">The selected authoring account.</param>
    /// <param name="categoryId">The selected blog category.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <param name="allowInactiveCategory">
    /// The category the edited row already belongs to. Keeping an inactive category is allowed while
    /// editing, so a post does not become unsavable just because its branch was switched off.
    /// </param>
    internal static async Task<Result> ValidateAsync(
        IUnitOfWork unitOfWork,
        UserId authorId,
        BlogCategoryId categoryId,
        CancellationToken ct,
        BlogCategoryId? allowInactiveCategory = null)
    {
        var category = await unitOfWork.BlogCategoryCommandRepository.GetByIdAsync(categoryId, ct);
        if (category is null)
            return BlogPostErrors.CategoryNotFound(categoryId);

        // An inactive category has been withdrawn from the picker. Filing a new post under it would
        // publish a row whose branch is hidden, so editing an old post has to move it first.
        if (!category.IsActive && categoryId != allowInactiveCategory)
            return BlogPostErrors.CategoryInactive(categoryId);

        var author = await unitOfWork.UserQueryRepository.GetByIdAsync(authorId, ct);
        if (author is null)
            return BlogPostErrors.AuthorNotFound(authorId);

        return Result.Success();
    }
}
