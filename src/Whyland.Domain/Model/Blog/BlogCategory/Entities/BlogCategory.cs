using System.Text.RegularExpressions;
using Whyland.Domain.Model.Blog.BlogCategory.Errors;
using Whyland.Shared.Results;

using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Domain.Model.Blog.BlogCategory.Entities;

/// <summary>
/// Hierarchical category used to organize blog posts (s12.1.1). Root categories have no parent.
/// Unlike the course catalogue, the blog taxonomy carries no depth or width ceiling: the SRS applies
/// the three-level cap only where it is explicitly configured, so a branch here may grow as deep as
/// the operator needs. What it may never do is loop back onto itself.
/// </summary>
public class BlogCategory : BaseEntity<BlogCategoryId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="LatinName"/>; also the database column size.</summary>
    public const int MaxLatinNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="Name"/>; also the database column size.</summary>
    public const int MaxNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="Slug"/>; also the database column size.</summary>
    public const int MaxSlugLength = 300;

    /// <summary>
    /// The level a root category sits on. There is no upper end — the blog taxonomy is deliberately
    /// uncapped (s12.1.1) — so this is the only bound the filter picker puts on the tree level.
    /// </summary>
    public const int MinDepth = 1;

    /// <summary>Slug shape: lower-case letters and digits separated by single hyphens.</summary>
    private static readonly Regex SlugPattern = new(
        @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    #endregion

    #region Properties

    /// <summary>
    /// The category name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Latin equivalent of the name, used to generate URL-friendly slugs.
    /// </summary>
    public string LatinName { get; private set; } = string.Empty;

    /// <summary>
    /// Unique, URL-friendly identifier of the category. Unique across the whole taxonomy, not only
    /// inside a sibling group: it is the segment the public blog URL is built from.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; counted between siblings, so the table shows each branch in this order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Identifier of the parent category; null for root categories. A category cannot be its own
    /// parent, and circular parenting is forbidden (s12.1.1).
    /// </summary>
    public BlogCategoryId? ParentId { get; private set; }

    /// <summary>
    /// Inactive categories must not be selectable as the category of new posts (s12.1.1). Switching
    /// one off leaves the posts already filed under it exactly where they are — the relation is never
    /// silently dropped — and switches off the branch below it so no live child is published under a
    /// parent that is hidden.
    /// </summary>
    public bool IsActive { get; private set; }

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
    /// The parent category, when this is a nested category.
    /// </summary>
    public BlogCategory? Parent { get; private set; }

    /// <summary>
    /// The child categories nested under this one.
    /// </summary>
    public ICollection<BlogCategory> Children { get; } = [];

    /// <summary>
    /// The posts filed under this category.
    /// </summary>
    public ICollection<BlogPostEntity> BlogPosts { get; } = [];

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private BlogCategory() { }

    /// <summary>
    /// Creates a new <see cref="BlogCategory"/> after trimming, validating and assigning the inputs.
    /// Whether the chosen parent exists, whether the slug is already taken and whether hanging under
    /// that parent would close a loop are questions about other rows — the handlers answer those.
    /// </summary>
    /// <param name="name">The category name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name. Will be trimmed.</param>
    /// <param name="slug">The unique URL segment. Will be trimmed and lower-cased.</param>
    /// <param name="displayOrder">The ordering weight inside its sibling group; zero or greater.</param>
    /// <param name="parentId">The parent category, or null for a root.</param>
    /// <param name="isActive">Whether the category is selectable for new posts.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new category,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<BlogCategory> Create(
        string name,
        string latinName,
        string slug,
        int displayOrder,
        BlogCategoryId? parentId,
        bool isActive)
    {
        var category = new BlogCategory();

        var applied = category.Apply(name, latinName, slug, displayOrder, parentId, isActive);
        return applied.IsFailure ? applied.Error : category;
    }

    /// <summary>
    /// Updates the mutable fields of an existing category after trimming, validating and assigning them.
    /// The display order and the parent are not part of this: the first belongs to drag &amp; drop
    /// (<see cref="SetDisplayOrder"/>) and the second to <see cref="SetParent"/>.
    /// </summary>
    /// <param name="name">The new name. Will be trimmed.</param>
    /// <param name="latinName">The new Latin name. Will be trimmed.</param>
    /// <param name="slug">The new URL segment. Will be trimmed and lower-cased.</param>
    /// <param name="isActive">Whether the category is selectable for new posts.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string name, string latinName, string slug, bool isActive)
        => Apply(name, latinName, slug, DisplayOrder, ParentId, isActive);

    /// <summary>
    /// Moves the category to a new position inside its sibling group, as both the form and the
    /// drag &amp; drop reorder rewrite the weight of every row in the group.
    /// </summary>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    public Result SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            return BlogCategoryErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>
    /// Hangs the category under a new parent (null moves it to the root level). The caller has already
    /// proved the parent exists and is not the row itself or one of its descendants.
    /// </summary>
    /// <param name="parentId">The new parent identifier, or null for the root level.</param>
    public void SetParent(BlogCategoryId? parentId) => ParentId = parentId;

    /// <summary>
    /// Switches the category off without touching its name or its posts — used when a disabled parent
    /// cascades down to its branch.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Marks the category as deleted, keeping the row for the posts still linked to it. The first
    /// deletion time wins; re-deleting an already hidden row leaves its original stamp intact.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Trims, validates and assigns every editable value. Shared by create and update so both paths
    /// enforce exactly the same rules; on failure the instance is left unchanged. Update hands in
    /// the current order and parent, so this method never moves a row on its own.
    /// </summary>
    /// <param name="name">The category name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name. Will be trimmed.</param>
    /// <param name="slug">The URL segment. Will be trimmed and lower-cased.</param>
    /// <param name="displayOrder">The ordering weight inside its sibling group; zero or greater.</param>
    /// <param name="parentId">The parent category, or null for a root.</param>
    /// <param name="isActive">Whether the category is selectable for new posts.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(
        string name,
        string latinName,
        string slug,
        int displayOrder,
        BlogCategoryId? parentId,
        bool isActive)
    {
        var trimmedName = name?.Trim();
        var trimmedLatinName = latinName?.Trim();
        var trimmedSlug = slug?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(trimmedName))
            return BlogCategoryErrors.FieldRequired(nameof(Name));

        if (trimmedName.Length > MaxNameLength)
            return BlogCategoryErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(trimmedLatinName))
            return BlogCategoryErrors.FieldRequired(nameof(LatinName));

        if (trimmedLatinName.Length > MaxLatinNameLength)
            return BlogCategoryErrors.FieldTooLong(nameof(LatinName), MaxLatinNameLength);

        if (string.IsNullOrWhiteSpace(trimmedSlug))
            return BlogCategoryErrors.FieldRequired(nameof(Slug));

        if (trimmedSlug.Length > MaxSlugLength)
            return BlogCategoryErrors.FieldTooLong(nameof(Slug), MaxSlugLength);

        if (!SlugPattern.IsMatch(trimmedSlug))
            return BlogCategoryErrors.InvalidSlug();

        if (displayOrder < 0)
            return BlogCategoryErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Name = trimmedName;
        LatinName = trimmedLatinName;
        Slug = trimmedSlug;
        DisplayOrder = displayOrder;
        ParentId = parentId;
        IsActive = isActive;

        return Result.Success();
    }

    #endregion
}