namespace Whyland.Application.Features.Admin.Faqs.Commands.Create;

/// <summary>
/// Handles <see cref="CreateFaqCommand"/> by creating the FAQ through the domain
/// factory method and persisting it via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to FAQ persistence.</param>
public sealed class CreateFaqCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateFaqCommand, long>
{
    /// <summary>
    /// Creates a new FAQ through the domain factory, or returns an error when
    /// the factory rejects the input or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new FAQ values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created FAQ, or an error describing the failure.</returns>
    public async Task<Result<long>> Handle(CreateFaqCommand command, CancellationToken ct)
    {
        var created = Faq.Create(command.Title, command.Description, command.DisplayOrder, command.IsActive);
        if(created.IsFailure)
            return created.Error;

        var faq = created.Data!;

        await unitOfWork.FaqCommandRepository.AddAsync(faq, ct);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return FaqErrors.CreateFailed();

        // Compact 1..n: whatever order the client picked (10, 17, 100, ...),
        // the FAQ lands on the sequence slot matching its sorted position.
        await unitOfWork.FaqCommandRepository.RenumberAsync(ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ?
            FaqErrors.CreateFailed() :
            faq.Id;
    }
}
