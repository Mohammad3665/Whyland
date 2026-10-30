namespace Whyland.Application.Features.Auth.Password.ChangePassword;

/// <summary>
/// Checks both passwords of the change form: the current one must be supplied, and the new one must
/// satisfy the shared strength rules.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>Current password is required; new password must satisfy the strength rules.</summary>
    public ChangePasswordCommandValidator(IOptions<AuthOptions> options)
    {
        RuleFor(x => x.CurrentPassword)
            .WhylandRequired()
            .WithName("رمز عبور فعلی");

        RuleFor(x => x.NewPassword)
            .WhylandRequired()
            .WhylandStrongPassword(options.Value.PasswordMinLength)
            .WithName("رمز عبور جدید");

        RuleFor(x => x)
            .Must(x => !string.Equals(x.CurrentPassword, x.NewPassword, StringComparison.Ordinal))
            .WithErrorCode(UserErrors.WrongPassword().Code)
            .WithMessage("رمز عبور جدید باید با رمز فعلی متفاوت باشد.");
    }
}
