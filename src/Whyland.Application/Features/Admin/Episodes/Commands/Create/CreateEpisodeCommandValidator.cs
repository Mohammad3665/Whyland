namespace Whyland.Application.Features.Admin.Episodes.Commands.Create;

/// <summary>
/// Validates <see cref="CreateEpisodeCommand"/> fields against the episode domain constraints.
/// </summary>
public sealed class CreateEpisodeCommandValidator : AbstractValidator<CreateEpisodeCommand>
{
    /// <summary>
    /// Configures the validation rules for every new episode field.
    /// </summary>
    public CreateEpisodeCommandValidator()
    {
        RuleFor(x => x.SectionId.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه سرفصل");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(Episode.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.Description!)
            .WhylandMaxLen(Episode.MaxDescriptionLength)
            .WithName("توضیحات");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}
