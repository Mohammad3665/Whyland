namespace Whyland.Application.Features.Admin.Episodes.Commands.UploadVideo;

/// <summary>
/// Validates <see cref="UploadEpisodeVideoCommand"/> before the file reaches protected storage.
/// The format, size and duration are enforced by the storage adapter; here only the request shape
/// is checked, so an empty upload never touches the disk.
/// </summary>
public sealed class UploadEpisodeVideoCommandValidator : AbstractValidator<UploadEpisodeVideoCommand>
{
    /// <summary>
    /// Configures the validation rules for the episode identifier and the uploaded file.
    /// </summary>
    public UploadEpisodeVideoCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Video)
            .NotNull()
            .WithMessage("فایل ویدیو انتخاب نشده است.");

        When(x => x.Video is not null, () =>
        {
            RuleFor(x => x.Video.FileName)
                .WhylandRequired()
                .WithName("فایل ویدیو");

            RuleFor(x => x.Video.Length)
                .WhylandGreaterThan(0)
                .WithName("حجم فایل ویدیو");
        });
    }
}
