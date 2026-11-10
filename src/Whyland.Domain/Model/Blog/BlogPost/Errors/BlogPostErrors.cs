using Whyland.Domain.Common.Errors;

using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Domain.Model.Blog.BlogPost.Errors;

/// <summary>
/// Every failure the <see cref="BlogPostEntity"/> aggregate can report, including the ones raised by
/// the handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs; the wording
/// lives in the localization catalog.
/// </para>
/// </summary>
public static class BlogPostErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<BlogPostEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<BlogPostEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The specified field carries a negative value where only zero or above is allowed.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError GreaterThanOrEqualToZero(string field) =>
        DomainError.For<BlogPostEntity>(nameof(GreaterThanOrEqualToZero), ErrorType.Validation, field: field);

    /// <summary>The slug is not a URL segment: it carries characters other than lower-case letters, digits and single hyphens.</summary>
    public static DomainError InvalidSlug() =>
        DomainError.For<BlogPostEntity>(nameof(InvalidSlug), ErrorType.Validation, nameof(BlogPostEntity.Slug));

    /// <summary>The display status is not one of the values the system knows.</summary>
    public static DomainError InvalidDisplayStatus() =>
        DomainError.For<BlogPostEntity>(nameof(InvalidDisplayStatus), ErrorType.Validation, nameof(BlogPostEntity.DisplayStatus));

    /// <summary>No blog post exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(BlogPostId id) =>
        DomainError.For<BlogPostEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>The selected blog category does not exist, or has been removed.</summary>
    /// <param name="categoryId">The category identifier that could not be resolved.</param>
    public static DomainError CategoryNotFound(BlogCategoryId categoryId) =>
        DomainError.For<BlogPostEntity>(nameof(CategoryNotFound), ErrorType.NotFound, field: null, ("CategoryId", categoryId));

    /// <summary>
    /// The selected blog category is switched off, so new posts must not be filed under it.
    /// </summary>
    /// <param name="categoryId">The disabled category identifier.</param>
    public static DomainError CategoryInactive(BlogCategoryId categoryId) =>
        DomainError.For<BlogPostEntity>(nameof(CategoryInactive), ErrorType.Validation, field: null, ("CategoryId", categoryId));

    /// <summary>The selected authoring account does not exist.</summary>
    /// <param name="authorId">The account identifier that could not be resolved.</param>
    public static DomainError AuthorNotFound(UserId authorId) =>
        DomainError.For<BlogPostEntity>(nameof(AuthorNotFound), ErrorType.NotFound, field: null, ("AuthorId", authorId));

    /// <summary>The slug is already used by another live blog post.</summary>
    /// <param name="slug">The slug that clashed.</param>
    public static DomainError SlugTaken(string slug) =>
        DomainError.For<BlogPostEntity>(nameof(SlugTaken), ErrorType.Validation, nameof(BlogPostEntity.Slug), ("Slug", slug));

    /// <summary>The post could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<BlogPostEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The post could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<BlogPostEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The post could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<BlogPostEntity>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The posts could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<BlogPostEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The new display order could not be saved (persistence failure).</summary>
    public static DomainError ReorderFailed() =>
        DomainError.For<BlogPostEntity>(nameof(ReorderFailed), ErrorType.Unexpected);
}
