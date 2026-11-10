namespace Whyland.Shared.Dtos.Blog.BlogPost;

/// <summary>
/// One row of the admin blog post table, and at the same time everything the edit form and details
/// modal need: the post's own fields, the resolved author and category, both public image URLs and
/// the audit/soft-delete columns. Opening a row is therefore a lookup in the rows the table already
/// holds and never a second round-trip.
/// </summary>
/// <param name="Id">The unique identifier of the post.</param>
/// <param name="Title">The post title.</param>
/// <param name="ShortDescription">The excerpt shown in listings.</param>
/// <param name="Content">The full article body from the rich-text editor.</param>
/// <param name="AuthorId">The authoring account identifier.</param>
/// <param name="AuthorName">Full name of the author, resolved by joining the account.</param>
/// <param name="CategoryId">The blog category identifier.</param>
/// <param name="CategoryName">Name of the category the post is filed under.</param>
/// <param name="Image">The stored relative image path, or null when no cover was uploaded.</param>
/// <param name="ImageUrl">Public URL of the stored cover image, or null.</param>
/// <param name="ThumbnailUrl">Public URL of its thumbnail, or null.</param>
/// <param name="DisplayOrder">Ordering weight inside the category.</param>
/// <param name="DisplayStatus">Visibility status as a string (Draft, Published, Hidden).</param>
/// <param name="Slug">The unique URL segment of the post.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identifier of the creating account.</param>
/// <param name="CreatedByName">Full name of the creating account, when it resolved.</param>
/// <param name="UpdatedAt">UTC timestamp of the last change.</param>
/// <param name="UpdatedBy">Identifier of the last editing account.</param>
/// <param name="UpdatedByName">Full name of the last editing account, when it resolved.</param>
/// <param name="IsDeleted">Whether the row is soft-deleted.</param>
/// <param name="DeletedAt">UTC timestamp of the soft delete, when there was one.</param>
/// <param name="DeletedBy">Identifier of the account that soft-deleted the row.</param>
/// <param name="DeletedByName">Full name of the account that soft-deleted the row, when it resolved.</param>
public sealed record BlogPostListItemDto(
    long Id,
    string Title,
    string ShortDescription,
    string Content,
    Guid AuthorId,
    string? AuthorName,
    long CategoryId,
    string? CategoryName,
    string? Image,
    string? ImageUrl,
    string? ThumbnailUrl,
    int DisplayOrder,
    string DisplayStatus,
    string Slug,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTime UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName,
    bool IsDeleted,
    DateTime? DeletedAt,
    Guid? DeletedBy,
    string? DeletedByName);
