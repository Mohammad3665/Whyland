namespace Whyland.Application.Features.Admin.Episodes.Commands.RemoveVideo;

/// <summary>
/// Command that detaches and deletes the protected video of one episode. Removing an episode that
/// has no video is a no-op, so the action is safe to repeat.
/// </summary>
/// <param name="Id">The episode whose video is removed.</param>
public sealed record RemoveEpisodeVideoCommand(EpisodeId Id) : ICommand;
