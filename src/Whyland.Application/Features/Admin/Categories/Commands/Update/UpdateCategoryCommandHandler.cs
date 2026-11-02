namespace Whyland.Application.Features.Admin.Categories.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateCategoryCommand"/> by loading the category, checking the hierarchy rules
/// that involve other rows (the new parent, loops, the depth of the whole branch, the sibling name),
/// applying the domain updates, and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to category persistence.</param>
public sealed class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateCategoryCommand>
{
    /// <summary>
    /// Updates the requested category. Switching a category off also switches off its whole branch,
    /// so a disabled parent never leaves live children above it on the site (s7.1.3).
    /// </summary>
    /// <param name="command">The command with the updated category values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.CategoryCommandRepository;

        var category = await repository.GetByIdAsync(command.Id, ct);
        if (category is null)
            return CategoryErrors.NotFound(command.Id);

        var all = await repository.GetLiveAllAsync(ct);
        var byId = all.ToDictionary(x => x.Id);
        var byParent = all.ToLookup(x => x.ParentId ?? CategoryHierarchy.NoParent);

        var parentId = command.ParentId;
        var isActive = command.IsActive;

        if (parentId is { } pid)
        {
            if (!byId.TryGetValue(pid, out var parent))
                return CategoryErrors.ParentNotFound(pid);

            // Either edge of the loop: the row becomes its own parent, or the chosen parent is
            // already somewhere below it.
            if (pid == command.Id || CategoryHierarchy.IsDescendant(pid, command.Id, byId))
                return CategoryErrors.CircularParent();

            // The branch travels with the row, so the ceiling is the new level of this category
            // plus the levels still hanging below it.
            var branchBelow = CategoryHierarchy.HeightOf(command.Id, byParent) - 1;
            if (CategoryHierarchy.DepthOf(pid, byId) + branchBelow > Category.MaxDepth)
                return CategoryErrors.MaxDepthExceeded(Category.MaxDepth);

            // A move needs a free slot under the new parent; staying with the current one does
            // not, because the row is already among the children being counted there.
            if (parentId != category.ParentId && !CategoryHierarchy.HasRoomForChild(all, pid))
                return CategoryErrors.MaxChildrenExceeded(Category.MaxChildren);

            if (!parent.IsActive)
                isActive = false;
        }

        if (await repository.IsNameTakenAsync(command.Name, parentId, command.Id, ct))
            return CategoryErrors.NameTaken(command.Name);

        var updated = category.Update(command.Name, command.LatinName, isActive);
        if (updated.IsFailure)
            return updated.Error;

        // Order and parent are their own intents: the form may resend the numbers the table already
        // shows, and a plain rename must not look like a move.
        category.SetDisplayOrder(command.DisplayOrder);
        if (parentId != category.ParentId)
            category.SetParent(parentId);

        // Disabling cascades down; enabling one row never silently enables a whole subtree —
        // the operator switches the children back on one by one.
        if (!isActive)
            await repository.DeactivateDescendantsAsync(command.Id, ct);

        repository.Update(category);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CategoryErrors.UpdateFailed();

        // Compact 1..n per sibling group so a client-entered order (10, 17, 100, ...) still
        // yields a clean sequence matching each category's sorted position.
        await repository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CategoryErrors.UpdateFailed() : Result.Success();
    }
}
