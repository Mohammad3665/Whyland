using Whyland.Application.Common.Interfaces.Identity;

namespace Whyland.Application.Features.Admin.Faqs.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteFaqCommand"/> by soft-deleting the FAQ through the
/// command repository and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ persistence.</param>
public sealed class DeleteFaqCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteFaqCommand>
{
    /// <summary>
    /// Soft-deletes the requested FAQ, or returns an error when it does not
    /// exist or persistence fails.
    /// </summary>
    /// <param name="command">The command containing the FAQ identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteFaqCommand command, CancellationToken ct)
    {
        var faq = await unitOfWork.FaqCommandRepository.GetByIdAsync(command.Id, ct);
        if (faq is null)
            return FaqErrors.NotFound(command.Id);

        unitOfWork.FaqCommandRepository.SoftDelete(faq);

        // First save persists the soft delete so the compaction below does not
        // include (and renumber around) the deleted row.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return FaqErrors.SoftDeleteFailed(command.Id);

        // Close any DisplayOrder gap left behind, then persist the new numbering.
        await unitOfWork.FaqCommandRepository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? FaqErrors.SoftDeleteFailed(command.Id) : Result.Success();
    }
}
