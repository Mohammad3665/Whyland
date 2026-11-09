namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteEpisodeAttachmentCommand"/> before deletion.
/// </summary>
public sealed class DeleteEpisodeAttachmentCommandValidator
    : AbstractValidator<DeleteEpisodeAttachmentCommand>
{
    /// <summary>
    /// Configures the validation rule for the attachment identifier.
    /// </summary>
    public DeleteEpisodeAttachmentCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
