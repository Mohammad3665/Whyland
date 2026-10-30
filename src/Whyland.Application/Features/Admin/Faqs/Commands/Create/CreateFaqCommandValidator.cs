namespace Whyland.Application.Features.Admin.Faqs.Commands.Create;

/// <summary>
/// Validates <see cref="CreateFaqCommand"/> fields against the FAQ domain constraints.
/// </summary>
public sealed class CreateFaqCommandValidator : AbstractValidator<CreateFaqCommand>
{
    /// <summary>
    /// Configures the validation rules for every new FAQ field.
    /// </summary>
    public CreateFaqCommandValidator()
    {
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
