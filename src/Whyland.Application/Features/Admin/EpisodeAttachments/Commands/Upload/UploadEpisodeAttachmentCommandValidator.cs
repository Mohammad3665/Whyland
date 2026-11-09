namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Upload;

/// <summary>
/// Validates <see cref="UploadEpisodeAttachmentCommand"/> before the file reaches protected storage.
/// The extension, signature and size are enforced by the storage adapter; here only the request
/// shape is checked, so an empty upload never touches the disk.
/// </summary>
public sealed class UploadEpisodeAttachmentCommandValidator
    : AbstractValidator<UploadEpisodeAttachmentCommand>
{
    /// <summary>
    /// Configures the validation rules for the episode identifier and the uploaded file.
    /// </summary>
    public UploadEpisodeAttachmentCommandValidator()
    {
        RuleFor(x => x.EpisodeId.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Attachment)
            .NotNull()
            .WithMessage("فایل پیوست انتخاب نشده است.");

        When(x => x.Attachment is not null, () =>
        {
            RuleFor(x => x.Attachment.FileName)
                .WhylandRequired()
                .WithName("فایل پیوست");

            RuleFor(x => x.Attachment.Length)
                .WhylandGreaterThan(0)
                .WithName("حجم فایل پیوست");
        });
    }
}
