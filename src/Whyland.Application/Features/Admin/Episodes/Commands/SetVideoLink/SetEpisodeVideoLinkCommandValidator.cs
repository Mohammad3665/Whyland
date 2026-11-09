namespace Whyland.Application.Features.Admin.Episodes.Commands.SetVideoLink;

/// <summary>
/// Validates <see cref="SetEpisodeVideoLinkCommand"/> before the link is stored. The URL scheme and
/// length are also enforced by the aggregate, so a value that bypasses validation still cannot land
/// in the column.
/// </summary>
public sealed class SetEpisodeVideoLinkCommandValidator : AbstractValidator<SetEpisodeVideoLinkCommand>
{
    /// <summary>
    /// Configures the validation rules for the episode identifier and the external link.
    /// </summary>
    public SetEpisodeVideoLinkCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Url)
            .WhylandRequired()
            .WithName("لینک ویدیو");

        RuleFor(x => x.Url)
            .WhylandMaxLen(Episode.MaxVideoUrlLength)
            .WithName("لینک ویدیو");

        RuleFor(x => x.Url)
            .Must(BeAnAbsoluteHttpUrl)
            .WithMessage("لینک ویدیو باید یک آدرس کامل با http یا https باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));
    }

    /// <summary>Whether the text is an absolute http(s) URL.</summary>
    private static bool BeAnAbsoluteHttpUrl(string? url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
