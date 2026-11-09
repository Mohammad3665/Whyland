namespace Whyland.Application.Features.Admin.Episodes.Commands.Update;

/// <summary>
/// Command that updates an existing episode's editable fields. The video is managed separately
/// through the upload and remove commands.
/// </summary>
/// <param name="Id">The identifier of the episode to update.</param>
/// <param name="Title">The new episode title.</param>
/// <param name="Description">The new optional description.</param>
/// <param name="DisplayOrder">The new display order used for sorting episodes.</param>
/// <param name="IsFree">Whether the episode is a free preview (s7.7.3).</param>
public sealed record UpdateEpisodeCommand(
    EpisodeId Id,
    string Title,
    string? Description,
    int DisplayOrder,
    bool IsFree) : ICommand;
