using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteEpisodeAttachmentCommand"/> by soft-deleting the metadata first and
/// removing the protected file afterwards: a database failure must never leave a row pointing at a
/// file that is already gone.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to attachment persistence.</param>
/// <param name="attachmentStorage">The protected storage that owns the file.</param>
/// <param name="logger">The logger used to record a file that could not be removed.</param>
public sealed class DeleteEpisodeAttachmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAttachmentStorageService attachmentStorage,
    ILogger<DeleteEpisodeAttachmentCommandHandler> logger)
    : ICommandHandler<DeleteEpisodeAttachmentCommand>
{
    /// <summary>
    /// Soft-deletes the requested attachment, or returns an error when it does not exist or
    /// persistence fails.
    /// </summary>
    /// <param name="command">The command containing the attachment identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteEpisodeAttachmentCommand command, CancellationToken ct)
    {
        var attachment = await unitOfWork.EpisodeAttachmentCommandRepository
            .GetByIdAsync(command.Id, ct);

        if (attachment is null)
            return EpisodeAttachmentErrors.NotFound(command.Id.Value);

        var storagePath = attachment.StoragePath;

        unitOfWork.EpisodeAttachmentCommandRepository.SoftDelete(attachment);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeAttachmentErrors.DeleteFailed();

        var deleted = await attachmentStorage.DeleteAsync(storagePath, ct);
        if (deleted.IsFailure)
            logger.LogWarning(
                "The deleted attachment file {StoragePath} could not be removed.",
                storagePath);

        return Result.Success();
    }
}
