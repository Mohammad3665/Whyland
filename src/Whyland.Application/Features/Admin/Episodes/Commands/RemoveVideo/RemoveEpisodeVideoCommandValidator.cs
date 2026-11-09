namespace Whyland.Application.Features.Admin.Episodes.Commands.RemoveVideo;

/// <summary>
/// Validates <see cref="RemoveEpisodeVideoCommand"/> before detaching the file.
/// </summary>
public sealed class RemoveEpisodeVideoCommandValidator : AbstractValidator<RemoveEpisodeVideoCommand>
{
    /// <summary>
    /// Configures the validation rule for the episode identifier.
    /// </summary>
    public RemoveEpisodeVideoCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
