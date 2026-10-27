namespace Whyland.Application.Features.Auth.Login.LoginWithPassword;

/// <summary>
/// Shape validation only: a password is never checked against the strength rules at login,
/// otherwise an account created before those rules existed could no longer sign in.
/// </summary>
public sealed class LoginWithPasswordCommandValidator : AbstractValidator<LoginWithPasswordCommand>
{
    /// <summary>A phone-shaped number and a password are both required.</summary>
    public LoginWithPasswordCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).WhylandRequired().WithName("شماره موبایل");
        RuleFor(x => x.Password).WhylandRequired().WithName("رمز عبور");
    }
}
