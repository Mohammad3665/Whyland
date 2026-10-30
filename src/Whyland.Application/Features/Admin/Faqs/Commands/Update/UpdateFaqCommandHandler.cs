namespace Whyland.Application.Features.Admin.Faqs.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateFaqCommand"/> by loading the FAQ, applying the domain
/// <c>Update</c> method, and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ persistence.</param>
public sealed class UpdateFaqCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateFaqCommand>
{
    /// <summary>
    /// Updates the requested FAQ, or returns an error when it does not exist,
    /// the domain update is rejected, or persistence fails.
    /// </summary>
    /// <param name="command">The command with the updated FAQ values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateFaqCommand command, CancellationToken ct)
    {
        var faq = await unitOfWork.FaqCommandRepository.GetByIdAsync(command.Id, ct);
        if(faq is null)
            return FaqErrors.NotFoundByTitle(command.Title);
        
        var updated = faq.Update(
            command.Title,
            command.Description,
            command.DisplayOrder,
            command.IsActive);

        if (updated.IsFailure)
            return updated.Error;

        unitOfWork.FaqCommandRepository.Update(faq);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return FaqErrors.UpdateFailed();

        // Compact 1..n so a client-entered order (10, 17, 100, ...) still
        // yields a clean sequence matching each FAQ's sorted position.
        await unitOfWork.FaqCommandRepository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? FaqErrors.UpdateFailed() : Result.Success();
    }
}
