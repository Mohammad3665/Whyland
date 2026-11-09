namespace Whyland.Application.Features.Admin.Episodes.Queries.GetForSection;

/// <summary>
/// Query that returns the live episodes of one section in display order, each with its video state
/// and attachment count.
/// </summary>
/// <param name="SectionId">The section whose episodes are read.</param>
public sealed record GetEpisodesQuery(CourseSectionId SectionId)
    : IQuery<IReadOnlyList<EpisodeListItemDto>>;
