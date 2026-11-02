namespace Whyland.Application.Features.Admin.Categories.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing category by its identifier.
/// </summary>
/// <param name="Id">The identifier of the category to soft-delete.</param>
public sealed record DeleteCategoryCommand(long Id) : ICommand;
