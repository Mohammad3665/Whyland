namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing blog category by its identifier.
/// </summary>
/// <param name="Id">The identifier of the category to soft-delete.</param>
public sealed record DeleteBlogCategoryCommand(BlogCategoryId Id) : ICommand;