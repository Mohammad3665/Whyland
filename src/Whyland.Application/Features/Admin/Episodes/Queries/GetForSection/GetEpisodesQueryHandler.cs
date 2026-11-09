namespace Whyland.Application.Features.Admin.Episodes.Queries.GetForSection;

/// <summary>
/// Handles <see cref="GetEpisodesQuery"/> by loading the episodes of one section through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodesQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetEpisodesQueryHandler> logger)
    : IQueryHandler<GetEpisodesQuery, IReadOnlyList<EpisodeListItemDto>>
{
    /// <summary>
    /// Returns the episodes ordered by DisplayOrder, or an error when the lookup fails.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query selecting the section whose episodes are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The read-only list of episodes, or an error.</returns>
    public async Task<Result<IReadOnlyList<EpisodeListItemDto>>> Handle(
        GetEpisodesQuery query,
        CancellationToken ct)
    {
        try
        {
            var episodes = await unitOfWork.EpisodeQueryRepository.GetForSectionAsync(query.SectionId, ct);
            return Result<IReadOnlyList<EpisodeListItemDto>>.Success(episodes);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading episodes of section {SectionId} failed.", query.SectionId.Value);
            return Result<IReadOnlyList<EpisodeListItemDto>>.Failure(EpisodeErrors.ListFailed());
        }
    }
}
