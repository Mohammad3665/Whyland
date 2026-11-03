
using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Domain.Model.Blog.BlogCategory.Entities;

/// <summary>
/// Hierarchical category used to organize blog posts (s4.10). Root categories have no parent.
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
    /// Unique, URL-friendly identifier of the category.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; categories are shown in this order (s4.10).
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Identifier of the parent category; null for root categories.
    /// </summary>
    public BlogCategoryId? ParentId { get; private set; }

    /// <summary>
    /// Inactive categories are hidden from navigation but keep their posts.
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
}
