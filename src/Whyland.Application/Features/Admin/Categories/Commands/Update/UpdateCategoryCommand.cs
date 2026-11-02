namespace Whyland.Application.Features.Admin.Categories.Commands.Update;

/// <summary>
/// Command that updates an existing category: its two names, its position and its state,
/// and — when the operator picked another box — the parent it hangs under.
/// </summary>
/// <param name="Id">The identifier of the category to update.</param>
/// <param name="Name">The new name.</param>
/// <param name="LatinName">The new Latin name used to build the slug.</param>
/// <param name="DisplayOrder">The new position inside its sibling group.</param>
/// <param name="ParentId">The new parent category, or null to move the row to the root level.</param>
/// <param name="IsActive">Whether the category is visible to end users.</param>
public sealed record UpdateCategoryCommand(
    long Id,
    string Name,
    string LatinName,
    int DisplayOrder,
    long? ParentId,
    bool IsActive) : ICommand;
