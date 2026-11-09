namespace Whyland.Application.Features.Admin.Episodes.Queries.GetSectionInfo;

/// <summary>
/// Query that returns the header data of one section for the episodes page: its title and the
/// course it belongs to.
/// </summary>
/// <param name="SectionId">The section being opened.</param>
public sealed record GetEpisodeSectionInfoQuery(CourseSectionId SectionId)
    : IQuery<EpisodeSectionInfoDto>;
