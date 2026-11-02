namespace Whyland.Domain.Entities.Blog;

/// <summary>
/// A blog article with author, category and display metadata (s4.10).
/// </summary>
public class BlogPost : BaseEntity<long>, ISoftDeletable
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
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// The category the post belongs to.
    /// </summary>
    public long CategoryId { get; private set; }

    /// <summary>
    /// Path or URL of the cover image; empty when no image is set.
    /// </summary>
    public string? Image { get; private set; }

    /// <summary>
    /// Manual ordering weight inside the category; posts are shown in this order.
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
    public Guid? DeletedBy { get; private set; }

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
    public User Author { get;  private set; } = null!;

    /// <summary>
    /// The category the post belongs to.
    /// </summary>
    public BlogCategory Category { get;  private set; } = null!;

    #endregion
}
