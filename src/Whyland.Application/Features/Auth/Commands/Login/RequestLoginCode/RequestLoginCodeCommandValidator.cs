namespace Whyland.Application.Features.Auth.Login.RequestLoginCode;

/// <summary>Validates the phone number shape before any work happens.</summary>
public sealed class RequestLoginCodeCommandValidator : AbstractValidator<RequestLoginCodeCommand>
{
    /// <summary>Configures phone validation: required, digits only, 10–15 chars.</summary>
    public RequestLoginCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .WhylandRequired()
            .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("شماره تلفن معتبر وارد کنید.")
            .WithName("شماره تلفن");
    }
}
