namespace Whyland.Application.Features.Admin.Episodes.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateEpisodeCommand"/> fields against the episode domain constraints.
/// </summary>
public sealed class UpdateEpisodeCommandValidator : AbstractValidator<UpdateEpisodeCommand>
{
    /// <summary>
    /// Configures the validation rules for every updatable episode field.
    /// </summary>
    public UpdateEpisodeCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

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
