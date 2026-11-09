namespace Whyland.Application.Features.Admin.Episodes.Queries.GetHeader;

/// <summary>
/// Handles <see cref="GetEpisodeHeaderQuery"/> by loading the episode header through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodeHeaderQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetEpisodeHeaderQueryHandler> logger)
    : IQueryHandler<GetEpisodeHeaderQuery, EpisodeHeaderInfoDto>
{
    /// <summary>
    /// Returns the episode header, or a not-found error when the episode does not exist.
    /// Infrastructure failures are logged and masked behind a generic error.
    /// </summary>
    /// <param name="query">The query selecting the episode.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The episode header, or an error.</returns>
    public async Task<Result<EpisodeHeaderInfoDto>> Handle(
        GetEpisodeHeaderQuery query,
        CancellationToken ct)
    {
        try
        {
            var header = await unitOfWork.EpisodeQueryRepository.GetHeaderInfoAsync(query.EpisodeId, ct);

            return header is null
                ? Result<EpisodeHeaderInfoDto>.Failure(EpisodeErrors.NotFound(query.EpisodeId.Value))
                : Result<EpisodeHeaderInfoDto>.Success(header);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading episode {EpisodeId} failed.", query.EpisodeId.Value);
            return Result<EpisodeHeaderInfoDto>.Failure(EpisodeErrors.ListFailed());
        }
    }
}
