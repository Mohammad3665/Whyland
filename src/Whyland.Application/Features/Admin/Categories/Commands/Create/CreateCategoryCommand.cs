namespace Whyland.Application.Features.Admin.Categories.Commands.Create;

/// <summary>
/// Command that creates a new category in the hierarchy, returning the identifier of the
/// created row.
/// </summary>
/// <param name="Name">The category name shown on the site.</param>
/// <param name="LatinName">The Latin name used to build the slug.</param>
/// <param name="DisplayOrder">The position inside its sibling group; the save compacts the group to 1..n.</param>
/// <param name="ParentId">The parent category, or null to create a root.</param>
/// <param name="IsActive">Whether the category is visible to end users.</param>
public sealed record CreateCategoryCommand(
    string Name,
    string LatinName,
    int DisplayOrder,
    long? ParentId,
    bool IsActive) : ICommand<long>;
