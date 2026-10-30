namespace Whyland.Application.Features.Auth.Registration.CompleteRegistration;

/// <summary>
/// Validates the registration-completion fields before any work happens.
/// </summary>
public sealed class CompleteRegistrationCommandValidator : AbstractValidator<CompleteRegistrationCommand>
{
    /// <summary>Configures rules: ticket required, first/last name required, password optional but strong when set.</summary>
    public CompleteRegistrationCommandValidator(IOptions<AuthOptions> options)
    {
        RuleFor(x => x.Ticket)
            .WhylandRequired()
            .WithName("تیکت ثبت‌نام");

        RuleFor(x => x.FirstName)
            .WhylandRequired()
            .WhylandMaxLen(User.MaxFirstNameLength)
            .WithName("نام");

        RuleFor(x => x.LastName)
            .WhylandRequired()
            .WhylandMaxLen(User.MaxLastNameLength)
            .WithName("نام خانوادگی");

        // Password is optional (an OTP-only account is valid); when provided it must pass the same
        // strength rules as change and reset, so the three flows cannot disagree about a password.
        var minLength = options.Value.PasswordMinLength;

        RuleFor(x => x.Password)
            .Must(password => string.IsNullOrWhiteSpace(password) || PasswordRules.Satisfies(password, minLength))
            .WithErrorCode(ValidationErrors.InvalidFormat.Code)
            .WithMessage($"رمز عبور باید حداقل {minLength} کاراکتر باشد و شامل حرف و رقم باشد.")
            .WithName("رمز عبور");
    }
}
