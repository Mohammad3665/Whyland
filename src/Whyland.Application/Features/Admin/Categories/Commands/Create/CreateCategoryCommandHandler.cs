namespace Whyland.Application.Features.Admin.Categories.Commands.Create;

/// <summary>
/// Handles <see cref="CreateCategoryCommand"/> by checking the rules that span more than the row
/// being inserted — the parent, the depth it implies and the name of the sibling group — then
/// creating the category through the domain factory and persisting it via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to category persistence.</param>
public sealed class CreateCategoryCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateCategoryCommand, CategoryId>
{
    /// <summary>
    /// Creates a new category, or returns an error when the parent is missing, the branch would grow
    /// past the allowed depth, the name is already used at that level, the factory rejects the input,
    /// or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new category values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created category, or an error describing the failure.</returns>
    public async Task<Result<CategoryId>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.CategoryCommandRepository;

        // One read of the whole (small) catalogue: it answers both "does the parent exist" and
        // "how deep would this new row sit".
        var all = await repository.GetLiveAllAsync(ct);
        var byId = all.ToDictionary(x => x.Id);

        // The request carries raw numbers, so every id is wrapped here once: the tree walks below
        // only ever compare identifiers.
        var parentId = command.ParentId is { } rawParent ? CategoryId.From(rawParent.Value) : (CategoryId?)null;
        var isActive = command.IsActive;

        if (parentId is { } pid)
        {
            if (!byId.TryGetValue(pid, out var parent))
                return CategoryErrors.ParentNotFound(pid);

            // The parent's own level is the ceiling check: a child of the last allowed level
            // would open a fourth one.
            if (CategoryHierarchy.DepthOf(pid, byId) >= Category.MaxDepth)
                return CategoryErrors.MaxDepthExceeded(Category.MaxDepth);

            // The branch is capped on its width as well as on its height: a parent that already
            // holds Category.MaxChildren live children has no slot left for this row.
            if (!CategoryHierarchy.HasRoomForChild(all, pid))
                return CategoryErrors.MaxChildrenExceeded(Category.MaxChildren);

            // A live child under a disabled parent would reach the site without its branch,
            // so a new row inherits the state of the parent above it.
            if (!parent.IsActive)
                isActive = false;
        }

        if (await repository.IsNameTakenAsync(command.Name, parentId, ct: ct))
            return CategoryErrors.NameTaken(command.Name);

        var created = Category.Create(command.Name, command.LatinName, command.DisplayOrder, parentId, isActive);
        if (created.IsFailure)
            return created.Error;

        var category = created.Data!;

        await repository.AddAsync(category, ct);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CategoryErrors.CreateFailed();

        // Compact 1..n inside every sibling group: whatever order the client picked (10, 17, 100, ...),
        // the category lands on the sequence slot matching its sorted position.
        await repository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ?
            CategoryErrors.CreateFailed() :
            category.Id;
    }
}
