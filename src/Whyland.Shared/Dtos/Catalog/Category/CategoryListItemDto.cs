namespace Whyland.Shared.Dtos.Catalog.Category;

/// <summary>
/// One row of the admin category table, and at the same time everything the details modal shows:
/// the category's own fields plus the audit and soft-delete columns, so opening the modal is a
/// lookup in the rows the table already holds and never a second round-trip.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="Name">The Persian name shown in the catalogue.</param>
/// <param name="LatinName">The Latin equivalent, used for slugs.</param>
/// <param name="DisplayOrder">Ordering weight; counts inside the sibling group, not globally.</param>
/// <param name="ParentId">The parent category, or null for a root.</param>
/// <param name="ParentName">The parent's name, or null when the row is a root.</param>
/// <param name="Depth">The level the category sits on; a root is 1 and the ceiling is 3.</param>
/// <param name="IsActive">Whether the category is visible on the site.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identifier of the user who created the row.</param>
/// <param name="UpdatedAt">UTC timestamp of the last modification.</param>
/// <param name="UpdatedBy">Identifier of the user who last changed the row.</param>
/// <param name="IsDeleted">Whether the row is soft-deleted.</param>
/// <param name="DeletedAt">UTC timestamp of the soft delete, when there was one.</param>
/// <param name="DeletedBy">Identifier of the user who soft-deleted the row.</param>
public sealed record CategoryListItemDto(
    long Id,
    string Name,
    string LatinName,
    int DisplayOrder,
    long? ParentId,
    string? ParentName,
    int Depth,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime UpdatedAt,
    Guid? UpdatedBy,
    bool IsDeleted,
    DateTime? DeletedAt,
    Guid? DeletedBy);
