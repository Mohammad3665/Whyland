namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Requires the code and a password that satisfies the shared strength rules: this path writes a
/// password, so the rules must hold here (unlike password login, which only ever verifies).
/// </summary>
public sealed class ForgotPasswordConfirmCommandValidator : AbstractValidator<ForgotPasswordConfirmCommand>
{
    /// <summary>Number, code and a strong new password are required.</summary>
    public ForgotPasswordConfirmCommandValidator(IOptions<AuthOptions> options)
    {
        RuleFor(x => x.PhoneNumber).WhylandRequired().WithName("شماره موبایل");

        RuleFor(x => x.Code)
            .Matches(@"^[0-9]{4,8}$")
                .WithMessage("کد دریافتی را درست وارد کنید.")
            .WithName("کد تأیید");

        RuleFor(x => x.NewPassword)
            .WhylandRequired()
            .WhylandStrongPassword(options.Value.PasswordMinLength)
            .WithName("رمز عبور جدید");
    }
}
