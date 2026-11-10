namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateBlogCategoryCommand"/> by loading the category, checking the hierarchy
/// rules that involve other rows (the new parent and the loop it would close, the slug it claims),
/// applying the domain updates, and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog category persistence.</param>
public sealed class UpdateBlogCategoryCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateBlogCategoryCommand>
{
    /// <summary>
    /// Updates the requested category. Switching a category off also switches off its whole branch,
    /// so a hidden parent never leaves selectable children above it; the posts already filed under
    /// any of those rows keep their category, which s12.1.1 requires.
    /// </summary>
    /// <param name="command">The command with the updated category values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateBlogCategoryCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.BlogCategoryCommandRepository;

        var category = await repository.GetByIdAsync(command.Id, ct);
        if (category is null)
            return BlogCategoryErrors.NotFound(command.Id);

        var parentId = command.ParentId;
        var isActive = command.IsActive;

        if (parentId is { } pid)
        {
            var all = await repository.GetLiveAllAsync(ct);
            var byId = all.ToDictionary(x => x.Id);

            if (!byId.TryGetValue(pid, out var parent))
                return BlogCategoryErrors.ParentNotFound(pid);

            // Either edge of the loop: the row becomes its own parent, or the chosen parent is
            // already somewhere below it. With no depth ceiling to stop at, this is the only tree
            // rule the blog taxonomy enforces (s12.1.1).
            if (pid == command.Id || BlogCategoryHierarchy.IsDescendant(pid, command.Id, byId))
                return BlogCategoryErrors.CircularParent();

            if (!parent.IsActive)
                isActive = false;
        }

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await repository.IsSlugTakenAsync(slug, command.Id, ct))
            return BlogCategoryErrors.SlugTaken(slug);

        var updated = category.Update(command.Name, command.LatinName, slug, isActive);
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
            return BlogCategoryErrors.UpdateFailed();

        // Compact 1..n per sibling group so a client-entered order (10, 17, 100, ...) still
        // yields a clean sequence matching each category's sorted position.
        await repository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? BlogCategoryErrors.UpdateFailed() : Result.Success();
    }
}