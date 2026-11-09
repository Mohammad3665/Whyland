namespace Whyland.Application.Features.Admin.Episodes.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the episodes of one section (s7.7).
/// </summary>
/// <param name="SectionId">The section whose episodes are reordered.</param>
/// <param name="OrderedIds">The episode identifiers in the desired visual order; index + 1 becomes DisplayOrder.</param>
public sealed record ReorderEpisodesCommand(
    CourseSectionId SectionId,
    IReadOnlyList<EpisodeId> OrderedIds) : ICommand;
