namespace Whyland.Application.Features.Admin.Episodes.Commands.SetVideoLink;

/// <summary>
/// Command that points one episode at an externally hosted video instead of a stored MP4.
/// </summary>
/// <param name="Id">The episode that receives the link.</param>
/// <param name="Url">The absolute http(s) link to the video.</param>
public sealed record SetEpisodeVideoLinkCommand(EpisodeId Id, string Url) : ICommand;
