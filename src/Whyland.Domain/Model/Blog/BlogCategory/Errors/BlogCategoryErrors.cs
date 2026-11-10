using Whyland.Domain.Common.Errors;

using BlogCategoryEntity = Whyland.Domain.Model.Blog.BlogCategory.Entities.BlogCategory;

namespace Whyland.Domain.Model.Blog.BlogCategory.Errors;

/// <summary>
/// Every failure the <see cref="BlogCategoryEntity"/> aggregate can report, including the ones raised by
/// the handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs; the wording
/// lives in the localization catalog.
/// </para>
/// </summary>
public static class BlogCategoryErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<BlogCategoryEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<BlogCategoryEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The specified field carries a negative value where only zero or above is allowed.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError GreaterThanOrEqualToZero(string field) =>
        DomainError.For<BlogCategoryEntity>(nameof(GreaterThanOrEqualToZero), ErrorType.Validation, field: field);

    /// <summary>The slug is not a URL segment: it carries characters other than lower-case letters, digits and single hyphens.</summary>
    public static DomainError InvalidSlug() =>
        DomainError.For<BlogCategoryEntity>(nameof(InvalidSlug), ErrorType.Validation, nameof(BlogCategoryEntity.Slug));

    /// <summary>No blog category exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(BlogCategoryId id) =>
        DomainError.For<BlogCategoryEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>
    /// The chosen parent does not exist: it was deleted, or it belongs to another aggregate.
    /// A separate code from <see cref="NotFound"/> because here the row being saved does exist and
    /// it is the reference that is broken.
    /// </summary>
    /// <param name="parentId">The parent identifier that could not be resolved.</param>
    public static DomainError ParentNotFound(BlogCategoryId parentId) =>
        DomainError.For<BlogCategoryEntity>(nameof(ParentNotFound), ErrorType.NotFound, field: null, ("ParentId", parentId));

    /// <summary>
    /// The slug is already used by another live blog category. Unlike the course catalogue's name,
    /// this one is unique across the whole taxonomy because it is the segment the public URL is built from.
    /// </summary>
    /// <param name="slug">The slug that clashed.</param>
    public static DomainError SlugTaken(string slug) =>
        DomainError.For<BlogCategoryEntity>(nameof(SlugTaken), ErrorType.Validation, nameof(BlogCategoryEntity.Slug), ("Slug", slug));

    /// <summary>
    /// The new parent is the category itself or one of its own descendants, which would make the
    /// tree un-walkable (s12.1.1).
    /// </summary>
    public static DomainError CircularParent() =>
        DomainError.For<BlogCategoryEntity>(nameof(CircularParent), ErrorType.Validation, nameof(BlogCategoryEntity.ParentId));

    /// <summary>The category could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<BlogCategoryEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The category could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<BlogCategoryEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The category could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<BlogCategoryEntity>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The categories could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<BlogCategoryEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>
    /// The category still has live children, so deleting it would leave a branch hanging on a parent
    /// that no longer exists.
    /// </summary>
    /// <param name="childCount">How many children are waiting above the category being deleted.</param>
    public static DomainError HasChildren(int childCount) =>
        DomainError.For<BlogCategoryEntity>(nameof(HasChildren), ErrorType.Validation, field: null, ("ChildCount", childCount));

    /// <summary>
    /// Live posts are still filed under the category, so deleting it would orphan the post-category
    /// relation the SRS forbids leaving behind (s12.1.1). The posts have to be moved or deleted first.
    /// </summary>
    /// <param name="postCount">How many live posts reference the category.</param>
    public static DomainError HasPosts(int postCount) =>
        DomainError.For<BlogCategoryEntity>(nameof(HasPosts), ErrorType.Validation, field: null, ("PostCount", postCount));

    /// <summary>The new display order could not be persisted (persistence failure); masked behind a generic message.</summary>
    public static DomainError ReorderFailed() =>
        DomainError.For<BlogCategoryEntity>(nameof(ReorderFailed), ErrorType.Unexpected);
}