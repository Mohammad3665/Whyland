namespace Whyland.Application.Features.Admin.Episodes.Queries.GetHeader;

/// <summary>
/// Query that returns the header data of one episode for the attachments page: its title and the
/// section and course it belongs to.
/// </summary>
/// <param name="EpisodeId">The episode being opened.</param>
public sealed record GetEpisodeHeaderQuery(EpisodeId EpisodeId) : IQuery<EpisodeHeaderInfoDto>;
