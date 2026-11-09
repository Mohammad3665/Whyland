using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Upload;

/// <summary>
/// Handles <see cref="UploadEpisodeAttachmentCommand"/> by enforcing the configured per-episode
/// limit, storing the file in protected storage under a server-generated name, and persisting its
/// metadata. A file written before a later step fails is removed again, so a failed save never
/// leaves an orphan behind.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode and attachment persistence.</param>
/// <param name="attachmentStorage">The protected storage that validates and owns the file.</param>
/// <param name="options">The configured attachment limits (s7.8).</param>
/// <param name="logger">The logger used to record a file that could not be removed.</param>
public sealed class UploadEpisodeAttachmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAttachmentStorageService attachmentStorage,
    IOptions<EpisodeAttachmentOptions> options,
    ILogger<UploadEpisodeAttachmentCommandHandler> logger)
    : ICommandHandler<UploadEpisodeAttachmentCommand, EpisodeAttachmentId>
{
    private readonly EpisodeAttachmentOptions _options = options.Value;

    /// <summary>
    /// Stores and attaches the file, or returns the limit/domain/storage error or a persistence
    /// failure.
    /// </summary>
    /// <param name="command">The episode identifier and the uploaded file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created attachment, or an error describing the failure.</returns>
    public async Task<Result<EpisodeAttachmentId>> Handle(
        UploadEpisodeAttachmentCommand command,
        CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.EpisodeId, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.EpisodeId.Value);

        var currentCount = await unitOfWork.EpisodeAttachmentCommandRepository
            .CountForEpisodeAsync(command.EpisodeId, ct);

        if (currentCount >= _options.MaxAttachmentsPerEpisode)
            return EpisodeAttachmentErrors.LimitReached(
                command.EpisodeId.Value,
                _options.MaxAttachmentsPerEpisode);

        var saved = await attachmentStorage.SaveAsync(
            command.Attachment.Content,
            command.Attachment.FileName,
            command.Attachment.ContentType,
            command.Attachment.Length,
            ct);

        if (saved.IsFailure)
            return saved.Error;

        var stored = saved.Data!;

        // The browser's name is display-only metadata: the storage key is generated server-side and
        // the download response never trusts this value as a path (s8.5).
        var displayName = attachmentStorage.SanitizeFileName(command.Attachment.FileName);
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = $"attachment{Path.GetExtension(stored.RelativePath)}";

        var created = EpisodeAttachment.Create(
            episode.Id,
            displayName,
            stored.RelativePath,
            stored.ContentType,
            stored.SizeBytes);

        if (created.IsFailure)
        {
            await DeleteQuietlyAsync(stored.RelativePath, ct);
            return created.Error;
        }

        var attachment = created.Data!;

        await unitOfWork.EpisodeAttachmentCommandRepository.AddAsync(attachment, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
        {
            await DeleteQuietlyAsync(stored.RelativePath, ct);
            return EpisodeAttachmentErrors.CreateFailed();
        }

        return attachment.Id;
    }

    /// <summary>Removes a file written for a request that then failed; a failure here is only logged.</summary>
    private async Task DeleteQuietlyAsync(string storagePath, CancellationToken ct)
    {
        var deleted = await attachmentStorage.DeleteAsync(storagePath, ct);
        if (deleted.IsFailure)
            logger.LogWarning("The orphaned attachment file {StoragePath} could not be removed.", storagePath);
    }
}
