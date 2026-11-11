namespace Whyland.Shared.Dtos.Blog.BlogCategory;

/// <summary>
/// One row of the admin blog category table, and at the same time everything the details modal shows:
/// the category's own fields plus the audit and soft-delete columns, so opening the modal is a
/// lookup in the rows the table already holds and never a second round-trip.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="Name">The Persian name shown in the blog taxonomy.</param>
/// <param name="LatinName">The Latin equivalent of the name.</param>
/// <param name="Slug">The unique URL segment of the category.</param>
/// <param name="DisplayOrder">Ordering weight; counts inside the sibling group, not globally.</param>
/// <param name="ParentId">The parent category, or null for a root.</param>
/// <param name="ParentName">The parent's name, or null when the row is a root.</param>
/// <param name="Depth">The level the category sits on; a root is 1 and there is no ceiling (s12.1.1).</param>
/// <param name="PostCount">How many live posts are filed under the category; the delete guard reads it.</param>
/// <param name="IsActive">Whether the category is selectable for new posts.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identifier of the user who created the row.</param>
/// <param name="CreatedByName">
/// The full name of that account, read by joining it to its key. The modal shows a name rather than
/// a guid an operator cannot read; null when the row has no author, which the page shows as a dash.
/// </param>
/// <param name="UpdatedAt">UTC timestamp of the last modification; null until the first update.</param>
/// <param name="UpdatedBy">Identifier of the user who last changed the row.</param>
/// <param name="UpdatedByName">The full name of the account behind <paramref name="UpdatedBy"/>.</param>
/// <param name="IsDeleted">Whether the row is soft-deleted.</param>
/// <param name="DeletedAt">UTC timestamp of the soft delete, when there was one.</param>
/// <param name="DeletedBy">Identifier of the user who soft-deleted the row.</param>
public sealed record BlogCategoryListItemDto(
    long Id,
    string Name,
    string LatinName,
    string Slug,
    int DisplayOrder,
    long? ParentId,
    string? ParentName,
    int Depth,
    int PostCount,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName,
    bool IsDeleted,
    DateTime? DeletedAt,
    Guid? DeletedBy);