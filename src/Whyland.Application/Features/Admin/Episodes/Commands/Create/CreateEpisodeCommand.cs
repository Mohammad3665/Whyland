namespace Whyland.Application.Features.Admin.Episodes.Commands.Create;

/// <summary>
/// Command that creates a new episode inside one section and returns the identifier of the created
/// episode.
/// </summary>
/// <param name="SectionId">The section the episode belongs to.</param>
/// <param name="Title">The episode title.</param>
/// <param name="Description">The optional description.</param>
/// <param name="DisplayOrder">The display order used for sorting episodes.</param>
/// <param name="IsFree">Whether the episode is a free preview (s7.7.3).</param>
public sealed record CreateEpisodeCommand(
    CourseSectionId SectionId,
    string Title,
    string? Description,
    int DisplayOrder,
    bool IsFree) : ICommand<EpisodeId>;
