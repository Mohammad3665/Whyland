namespace Whyland.Application.Features.Auth.Login.VerifyLoginCode;

/// <summary>Basic shape validation for the login-code verification step.</summary>
public sealed class VerifyLoginCodeCommandValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    /// <summary>Phone required; code required and exactly six digits.</summary>
    public VerifyLoginCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).WhylandRequired().WithName("شماره تلفن");
        RuleFor(x => x.Code)
            .WhylandRequired()
            .Matches(@"^[0-9]{6}$")
                .WithMessage("کد تأیید باید ۶ رقم باشد.")
            .WithName("کد تأیید");
    }
}
