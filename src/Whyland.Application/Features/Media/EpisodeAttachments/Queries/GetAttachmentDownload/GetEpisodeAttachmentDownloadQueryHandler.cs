using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Media.EpisodeAttachments.Queries.GetAttachmentDownload;

/// <summary>
/// Handles <see cref="GetEpisodeAttachmentDownloadQuery"/> by resolving the attachment and its
/// episode, deciding access before touching the disk, and only then opening the protected file.
/// <para>
/// Attachments never inherit the free-preview exemption (s8.2.1): a caller needs the course itself,
/// or the course-management permission. A hidden or draft course stays closed to everyone else.
/// </para>
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to attachment and episode query data.</param>
/// <param name="attachmentStorage">The protected storage that owns the attachment files.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodeAttachmentDownloadQueryHandler(
    IUnitOfWork unitOfWork,
    IAttachmentStorageService attachmentStorage,
    ILogger<GetEpisodeAttachmentDownloadQueryHandler> logger)
    : IQueryHandler<GetEpisodeAttachmentDownloadQuery, EpisodeAttachmentFile>
{
    /// <summary>
    /// Returns an open stream of the attachment, or an error when the attachment or its file does
    /// not exist, the caller may not download it, or the file cannot be opened.
    /// </summary>
    /// <param name="query">The attachment identifier and the caller's access inputs.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The open file stream with its download metadata, or an error describing the failure.</returns>
    public async Task<Result<EpisodeAttachmentFile>> Handle(
        GetEpisodeAttachmentDownloadQuery query,
        CancellationToken ct)
    {
        var download = await unitOfWork.EpisodeAttachmentQueryRepository
            .GetDownloadInfoAsync(query.AttachmentId, ct);

        if (download is null)
            return EpisodeAttachmentErrors.NotFound(query.AttachmentId.Value);

        var access = await unitOfWork.EpisodeQueryRepository.GetAccessInfoAsync(
            EpisodeId.From(download.EpisodeId),
            ct);

        if (access is null)
            return EpisodeAttachmentErrors.NotFound(query.AttachmentId.Value);

        if (!query.IsStaff)
        {
            if (access.CourseDisplayStatus != DisplayStatus.Published)
                return EpisodeAttachmentErrors.AccessDenied();

            var entitled = query.UserId is { } userId
                && await unitOfWork.EpisodeQueryRepository.HasCourseAccessAsync(
                    CourseId.From(access.CourseId),
                    userId,
                    ct);

            if (!entitled)
                return EpisodeAttachmentErrors.AccessDenied();
        }

        var opened = await attachmentStorage.OpenAsync(download.StoragePath, ct);
        if (opened.IsFailure)
        {
            logger.LogWarning(
                "The file of attachment {AttachmentId} could not be opened.",
                query.AttachmentId.Value);

            return Result<EpisodeAttachmentFile>.Failure(opened.Error);
        }

        return Result<EpisodeAttachmentFile>.Success(
            new EpisodeAttachmentFile(opened.Data!.Content, download.FileName, download.ContentType));
    }
}
