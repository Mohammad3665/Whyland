namespace Whyland.Application.Features.Admin.Categories.Commands.Reorder;

/// <summary>
/// Handles <see cref="ReorderCategoriesCommand"/> by rewriting DisplayOrder over the posted visual
/// sequence and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to category persistence.</param>
public sealed class ReorderCategoriesCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<ReorderCategoriesCommand>
{
    /// <summary>
    /// Persists the new display order, or returns an error when persistence fails.
    /// </summary>
    /// <param name="command">The command carrying the new ordering sequence.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(ReorderCategoriesCommand command, CancellationToken ct)
    {
        await unitOfWork.CategoryCommandRepository.ReorderAsync(command.OrderedIds, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CategoryErrors.ReorderFailed() : Result.Success();
    }
}
