namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Create;

/// <summary>
/// Command that creates a new blog category in the hierarchy, returning the identifier of the
/// created row.
/// </summary>
/// <param name="Name">The category name shown on the site.</param>
/// <param name="LatinName">The Latin equivalent of the name.</param>
/// <param name="Slug">The unique URL segment of the category.</param>
/// <param name="DisplayOrder">The position inside its sibling group; the save compacts the group to 1..n.</param>
/// <param name="ParentId">The parent category, or null to create a root.</param>
/// <param name="IsActive">Whether the category is selectable for new posts.</param>
public sealed record CreateBlogCategoryCommand(
    string Name,
    string LatinName,
    string Slug,
    int DisplayOrder,
    BlogCategoryId? ParentId,
    bool IsActive) : ICommand<BlogCategoryId>;