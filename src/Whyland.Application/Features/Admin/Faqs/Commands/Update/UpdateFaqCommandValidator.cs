namespace Whyland.Application.Features.Admin.Faqs.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateFaqCommand"/> fields against the FAQ domain constraints.
/// </summary>
public sealed class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    /// <summary>
    /// Configures the validation rules for every updatable FAQ field.
    /// </summary>
    public UpdateFaqCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(Faq.MaxTitleLength)
            .WithName("عنوان");

        RuleFor(x => x.Description)
            .WhylandRequired()
            .WhylandMaxLen(Faq.MaxDescriptionLength)
            .WithName("توضیحات");

        RuleFor(x => x.DisplayOrder)
            .WhylandGreaterThanOrEqualTo(0)
            .WithName("ترتیب نمایش");
    }
}