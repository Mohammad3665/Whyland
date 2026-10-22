
using Whyland.Domain.Common.Primitives;

namespace Whyland.Domain.Entities.Blog;

/// <summary>
/// Hierarchical category used to organize blog posts (s4.10). Root categories have no parent.
/// </summary>
public class BlogCategory : BaseEntity<long>, ISoftDeletable
{
    #region Properties

    /// <summary>
    /// The category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Latin equivalent of the name, used to generate URL-friendly slugs.
    /// </summary>
    public string LatinName { get; set; } = string.Empty;

    /// <summary>
    /// Unique, URL-friendly identifier of the category.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; categories are shown in this order (s4.10).
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Identifier of the parent category; null for root categories.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Inactive categories are hidden from navigation but keep their posts.
    /// </summary>
    public bool IsActive { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The parent category, when this is a nested category.
    /// </summary>
    public BlogCategory? Parent { get; set; }

    /// <summary>
    /// The child categories nested under this one.
    /// </summary>
    public ICollection<BlogCategory> Children { get; set; } = [];

    /// <summary>
    /// The posts filed under this category.
    /// </summary>
    public ICollection<BlogPost> BlogPosts { get; set; } = [];

    #endregion
}
