using System.Text.RegularExpressions;
using Whyland.Domain.Model.Blog.BlogPost.Errors;
using Whyland.Shared.Results;
using Whyland.Shared.Enums;

using BlogCategoryEntity = Whyland.Domain.Model.Blog.BlogCategory.Entities.BlogCategory;

namespace Whyland.Domain.Model.Blog.BlogPost.Entities;

/// <summary>
/// A blog article with author, category and display metadata (s4.10).
/// </summary>
public class BlogPost : BaseEntity<BlogPostId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Image"/>; also the database column size.</summary>
    public const int MaxImageLength = 512;

    /// <summary>Maximum allowed length of <see cref="ShortDescription"/>; also the database column size.</summary>
    public const int MaxShortDescriptionLength = 500;

    /// <summary>Maximum allowed length of <see cref="Slug"/>; also the database column size.</summary>
    public const int MaxSlugLength = 300;

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Slug shape: lower-case letters and digits separated by single hyphens.</summary>
    private static readonly Regex SlugPattern = new(
        @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    #endregion

    #region Properties

    /// <summary>
    /// The post title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Short excerpt shown in listings before the full content.
    /// </summary>
    public string ShortDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Full body of the article.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// The authoring user.
    /// </summary>
    public UserId AuthorId { get; private set; }

    /// <summary>
    /// The category the post belongs to. The blog taxonomy is its own aggregate, so this is a
    /// <see cref="BlogCategoryId"/>, not the catalogue's <c>CategoryId</c> — the two names differ
    /// only by the prefix.
    /// </summary>
    public BlogCategoryId CategoryId { get; private set; }

    /// <summary>
    /// Path or URL of the cover image; empty when no image is set.
    /// </summary>
    public string? Image { get; private set; }

    /// <summary>
    /// Manual ordering weight in the article listing; posts are shown in this order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Visibility state of the post (e.g. draft vs. published).
    /// </summary>
    public DisplayStatus DisplayStatus { get; private set; }

    /// <summary>
    /// Unique, URL-friendly identifier of the post.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public UserId? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The authoring user.
    /// </summary>
    public User Author { get; private set; } = null!;

    /// <summary>
    /// The category the post belongs to.
    /// </summary>
    public BlogCategoryEntity Category { get; private set; } = null!;

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private BlogPost() { }

    /// <summary>
    /// Creates a new <see cref="BlogPost"/> after trimming, validating and assigning the inputs.
    /// Whether the author, category and slug are available are questions about other rows, so the
    /// handlers answer those before calling this factory.
    /// </summary>
    /// <param name="title">The post title. Will be trimmed.</param>
    /// <param name="shortDescription">The listing excerpt. Will be trimmed.</param>
    /// <param name="content">The full article body.</param>
    /// <param name="authorId">The authoring account.</param>
    /// <param name="categoryId">The category the post is filed under.</param>
    /// <param name="slug">The unique URL segment. Will be trimmed and lower-cased.</param>
    /// <param name="displayOrder">The ordering weight in the article listing; zero or greater.</param>
    /// <param name="displayStatus">The public visibility state.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new post,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<BlogPost> Create(
        string title,
        string shortDescription,
        string content,
        UserId authorId,
        BlogCategoryId categoryId,
        string slug,
        int displayOrder,
        DisplayStatus displayStatus)
    {
        var post = new BlogPost();
        var applied = post.Apply(
            title, shortDescription, content, authorId, categoryId, slug, displayOrder, displayStatus);

        return applied.IsFailure ? applied.Error : post;
    }

    /// <summary>
    /// Updates the mutable fields of an existing post after trimming, validating and assigning them.
    /// </summary>
    /// <param name="title">The new title. Will be trimmed.</param>
    /// <param name="shortDescription">The new listing excerpt. Will be trimmed.</param>
    /// <param name="content">The new article body.</param>
    /// <param name="authorId">The new authoring account.</param>
    /// <param name="categoryId">The new category.</param>
    /// <param name="slug">The new URL segment. Will be trimmed and lower-cased.</param>
    /// <param name="displayOrder">The new ordering weight; zero or greater.</param>
    /// <param name="displayStatus">The new visibility state.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(
        string title,
        string shortDescription,
        string content,
        UserId authorId,
        BlogCategoryId categoryId,
        string slug,
        int displayOrder,
        DisplayStatus displayStatus)
        => Apply(title, shortDescription, content, authorId, categoryId, slug, displayOrder, displayStatus);

    /// <summary>
    /// Stores the relative path of the uploaded cover image, or clears it when null is passed.
    /// </summary>
    /// <param name="image">The stored relative path, or null to remove the image.</param>
    public Result SetImage(string? image)
    {
        var normalized = string.IsNullOrWhiteSpace(image) ? null : image.Trim();

        if (normalized is { Length: > MaxImageLength })
            return BlogPostErrors.FieldTooLong(nameof(Image), MaxImageLength);

        Image = normalized;
        return Result.Success();
    }

    /// <summary>
    /// Moves the post to a new position in the listing, as the reorder operation rewrites the
    /// weight of every post in the group.
    /// </summary>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    public Result SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            return BlogPostErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>
    /// Marks the post as deleted, keeping the row for its audit trail. The first deletion time wins;
    /// re-deleting an already hidden row leaves its original stamp intact.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Trims, validates and assigns every editable value. Shared by create and update so both paths
    /// enforce exactly the same rules; on failure the instance is left unchanged.
    /// </summary>
    private Result Apply(
        string title,
        string shortDescription,
        string content,
        UserId authorId,
        BlogCategoryId categoryId,
        string slug,
        int displayOrder,
        DisplayStatus displayStatus)
    {
        var trimmedTitle = title?.Trim();
        var trimmedShortDescription = shortDescription?.Trim();
        var trimmedContent = content?.Trim();
        var trimmedSlug = slug?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
            return BlogPostErrors.FieldRequired(nameof(Title));

        if (trimmedTitle.Length > MaxTitleLength)
            return BlogPostErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (string.IsNullOrWhiteSpace(trimmedShortDescription))
            return BlogPostErrors.FieldRequired(nameof(ShortDescription));

        if (trimmedShortDescription.Length > MaxShortDescriptionLength)
            return BlogPostErrors.FieldTooLong(nameof(ShortDescription), MaxShortDescriptionLength);

        if (string.IsNullOrWhiteSpace(trimmedContent))
            return BlogPostErrors.FieldRequired(nameof(Content));

        if (authorId == default)
            return BlogPostErrors.FieldRequired(nameof(AuthorId));

        if (categoryId == default)
            return BlogPostErrors.FieldRequired(nameof(CategoryId));

        if (string.IsNullOrWhiteSpace(trimmedSlug))
            return BlogPostErrors.FieldRequired(nameof(Slug));

        if (trimmedSlug.Length > MaxSlugLength)
            return BlogPostErrors.FieldTooLong(nameof(Slug), MaxSlugLength);

        if (!SlugPattern.IsMatch(trimmedSlug))
            return BlogPostErrors.InvalidSlug();

        if (displayOrder < 0)
            return BlogPostErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        if (!Enum.IsDefined(displayStatus))
            return BlogPostErrors.InvalidDisplayStatus();

        Title = trimmedTitle;
        ShortDescription = trimmedShortDescription;
        Content = trimmedContent;
        AuthorId = authorId;
        CategoryId = categoryId;
        Slug = trimmedSlug;
        DisplayOrder = displayOrder;
        DisplayStatus = displayStatus;

        return Result.Success();
    }

    #endregion
}
