namespace Whyland.Application.Features.Admin.Episodes.Queries.GetSectionInfo;

/// <summary>
/// Handles <see cref="GetEpisodeSectionInfoQuery"/> by loading the section header through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodeSectionInfoQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetEpisodeSectionInfoQueryHandler> logger)
    : IQueryHandler<GetEpisodeSectionInfoQuery, EpisodeSectionInfoDto>
{
    /// <summary>
    /// Returns the section header, or a not-found error when the section does not exist.
    /// Infrastructure failures are logged and masked behind a generic error.
    /// </summary>
    /// <param name="query">The query selecting the section.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The section header, or an error.</returns>
    public async Task<Result<EpisodeSectionInfoDto>> Handle(
        GetEpisodeSectionInfoQuery query,
        CancellationToken ct)
    {
        try
        {
            var section = await unitOfWork.EpisodeQueryRepository
                .GetSectionInfoAsync(query.SectionId, ct);

            return section is null
                ? Result<EpisodeSectionInfoDto>.Failure(CourseSectionErrors.NotFound(query.SectionId))
                : Result<EpisodeSectionInfoDto>.Success(section);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading section {SectionId} failed.", query.SectionId.Value);
            return Result<EpisodeSectionInfoDto>.Failure(EpisodeErrors.ListFailed());
        }
    }
}
