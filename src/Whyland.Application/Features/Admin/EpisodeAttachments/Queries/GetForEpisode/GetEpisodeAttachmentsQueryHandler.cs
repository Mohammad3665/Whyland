namespace Whyland.Application.Features.Admin.EpisodeAttachments.Queries.GetForEpisode;

/// <summary>
/// Handles <see cref="GetEpisodeAttachmentsQuery"/> by loading the attachments of one episode
/// through the Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to attachment query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodeAttachmentsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetEpisodeAttachmentsQueryHandler> logger)
    : IQueryHandler<GetEpisodeAttachmentsQuery, IReadOnlyList<EpisodeAttachmentListItemDto>>
{
    /// <summary>
    /// Returns the attachments in upload order, or an error when the lookup fails.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query selecting the episode whose attachments are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The read-only list of attachments, or an error.</returns>
    public async Task<Result<IReadOnlyList<EpisodeAttachmentListItemDto>>> Handle(
        GetEpisodeAttachmentsQuery query,
        CancellationToken ct)
    {
        try
        {
            var attachments = await unitOfWork.EpisodeAttachmentQueryRepository
                .GetForEpisodeAsync(query.EpisodeId, ct);

            return Result<IReadOnlyList<EpisodeAttachmentListItemDto>>.Success(attachments);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(
                ex,
                "Reading attachments of episode {EpisodeId} failed.",
                query.EpisodeId.Value);

            return Result<IReadOnlyList<EpisodeAttachmentListItemDto>>.Failure(
                EpisodeAttachmentErrors.ListFailed());
        }
    }
}
