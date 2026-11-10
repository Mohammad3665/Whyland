namespace Whyland.Application.Features.Admin.BlogCategories.Commands.Create;

/// <summary>
/// Handles <see cref="CreateBlogCategoryCommand"/> by checking the rules that span more than the row
/// being inserted — the parent it hangs under and the slug it claims — then creating the category
/// through the domain factory and persisting it via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog category persistence.</param>
public sealed class CreateBlogCategoryCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBlogCategoryCommand, BlogCategoryId>
{
    /// <summary>
    /// Creates a new blog category, or returns an error when the parent is missing, the slug is
    /// already used, the factory rejects the input, or persistence fails. The taxonomy has no depth
    /// or width ceiling (s12.1.1), so those are not among the rules judged here.
    /// </summary>
    /// <param name="command">The command with the new category values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created category, or an error describing the failure.</returns>
    public async Task<Result<BlogCategoryId>> Handle(CreateBlogCategoryCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.BlogCategoryCommandRepository;

        var parentId = command.ParentId;
        var isActive = command.IsActive;

        if (parentId is { } pid)
        {
            // One read of the whole taxonomy answers "does the parent exist"; the same read is what
            // the update handler walks for loops, so both judge the tree from identical data.
            var all = await repository.GetLiveAllAsync(ct);
            var parent = all.FirstOrDefault(x => x.Id == pid);
            if (parent is null)
                return BlogCategoryErrors.ParentNotFound(pid);

            // A live child under a disabled parent would be selectable for new posts while its branch
            // is hidden, so a new row inherits the state of the parent above it.
            if (!parent.IsActive)
                isActive = false;
        }

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await repository.IsSlugTakenAsync(slug, ct: ct))
            return BlogCategoryErrors.SlugTaken(slug);

        var created = BlogCategory.Create(command.Name, command.LatinName, slug, command.DisplayOrder, parentId, isActive);
        if (created.IsFailure)
            return created.Error;

        var category = created.Data!;

        await repository.AddAsync(category, ct);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return BlogCategoryErrors.CreateFailed();

        // Compact 1..n inside every sibling group: whatever order the client picked (10, 17, 100, ...),
        // the category lands on the sequence slot matching its sorted position.
        await repository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? BlogCategoryErrors.CreateFailed() : category.Id;
    }
}