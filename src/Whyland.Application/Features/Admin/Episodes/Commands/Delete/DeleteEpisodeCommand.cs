namespace Whyland.Application.Features.Admin.Episodes.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing episode by its identifier.
/// </summary>
/// <param name="Id">The identifier of the episode to soft-delete.</param>
public sealed record DeleteEpisodeCommand(EpisodeId Id) : ICommand;
