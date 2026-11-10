namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Update;

/// <summary>
/// Command that updates an existing blog category: its two names, its slug, its position and its
/// state, and — when the operator picked another box — the parent it hangs under.
/// </summary>
/// <param name="Id">The identifier of the category to update.</param>
/// <param name="Name">The new name.</param>
/// <param name="LatinName">The new Latin name.</param>
/// <param name="Slug">The new unique URL segment.</param>
/// <param name="DisplayOrder">The new position inside its sibling group.</param>
/// <param name="ParentId">The new parent category, or null to move the row to the root level.</param>
/// <param name="IsActive">Whether the category is selectable for new posts.</param>
public sealed record UpdateBlogCategoryCommand(
    BlogCategoryId Id,
    string Name,
    string LatinName,
    string Slug,
    int DisplayOrder,
    BlogCategoryId? ParentId,
    bool IsActive) : ICommand;