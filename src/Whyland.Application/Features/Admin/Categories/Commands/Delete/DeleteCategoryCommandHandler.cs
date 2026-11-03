namespace Whyland.Application.Features.Admin.Categories.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteCategoryCommand"/> by soft-deleting the category through the
/// command repository and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to category persistence.</param>
public sealed class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteCategoryCommand>
{
    /// <summary>
    /// Soft-deletes the requested category, or returns an error when it does not exist, still has
    /// children, or persistence fails. A branch is deleted from the bottom up: removing the parent
    /// would leave live children with no category above them.
    /// </summary>
    /// <param name="command">The command containing the category identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.CategoryCommandRepository;

        var category = await repository.GetByIdAsync(command.Id, ct);
        if (category is null)
            return CategoryErrors.NotFound(command.Id);

        var childCount = (await repository.GetLiveAllAsync(ct))
            .Count(x => x.ParentId == CategoryId.From(command.Id));

        if (childCount > 0)
            return CategoryErrors.HasChildren(childCount);

        repository.SoftDelete(category);

        // First save persists the soft delete so the compaction below does not
        // include (and renumber around) the deleted row.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CategoryErrors.SoftDeleteFailed();

        // Close any DisplayOrder gap left behind inside the sibling group, then persist the new numbering.
        await repository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CategoryErrors.SoftDeleteFailed() : Result.Success();
    }
}
