using Microsoft.Extensions.Options;
using Whyland.Application.Common.Options;
using Whyland.Application.Common.Validation;

namespace Whyland.Application.Features.Auth.Password.SetPassword;

/// <summary>
/// Checks the shape of the first password. Whether a phone number and its code are needed depends on
/// the stored account state, which only the handler can see, so that check lives there instead.
/// </summary>
public sealed class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    /// <summary>New password is required and must satisfy the strength rules.</summary>
    public SetPasswordCommandValidator(IOptions<AuthOptions> options)
    {
        RuleFor(x => x.NewPassword)
            .WhylandRequired()
            .WhylandStrongPassword(options.Value.PasswordMinLength)
            .WithName("رمز عبور جدید");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("شماره تلفن معتبر وارد کنید.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithName("شماره تلفن");
    }
}
