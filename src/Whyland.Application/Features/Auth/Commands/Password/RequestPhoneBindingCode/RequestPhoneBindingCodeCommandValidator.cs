namespace Whyland.Application.Features.Auth.Password.RequestPhoneBindingCode;

/// <summary>Validates the phone number shape before any SMS is sent.</summary>
public sealed class RequestPhoneBindingCodeCommandValidator : AbstractValidator<RequestPhoneBindingCodeCommand>
{
    /// <summary>Configures phone validation: required, digits only, 10–15 chars.</summary>
    public RequestPhoneBindingCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .WhylandRequired()
            .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("شماره تلفن معتبر وارد کنید.")
            .WithName("شماره تلفن");
    }
}
