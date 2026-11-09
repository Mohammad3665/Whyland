namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Delete;

/// <summary>
/// Command that soft-deletes one episode attachment and removes its protected file.
/// </summary>
/// <param name="Id">The identifier of the attachment to delete.</param>
public sealed record DeleteEpisodeAttachmentCommand(EpisodeAttachmentId Id) : ICommand;
