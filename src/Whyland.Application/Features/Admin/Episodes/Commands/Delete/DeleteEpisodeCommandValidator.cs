namespace Whyland.Application.Features.Admin.Episodes.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteEpisodeCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteEpisodeCommandValidator : AbstractValidator<DeleteEpisodeCommand>
{
    /// <summary>
    /// Configures the validation rule for the episode identifier.
    /// </summary>
    public DeleteEpisodeCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
