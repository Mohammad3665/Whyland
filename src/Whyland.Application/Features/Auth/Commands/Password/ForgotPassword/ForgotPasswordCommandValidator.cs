namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Shape validation only: whether the number belongs to an account is decided by the handler,
/// and the answer must not depend on it.
/// </summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>A phone-shaped number is required.</summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).WhylandRequired().WithName("شماره موبایل");
    }
}
