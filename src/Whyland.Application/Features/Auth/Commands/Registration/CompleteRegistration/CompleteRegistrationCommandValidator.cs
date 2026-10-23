namespace Whyland.Application.Features.Auth.Registration.CompleteRegistration;

/// <summary>Validates the registration-completion fields before any work happens.</summary>
public sealed class CompleteRegistrationCommandValidator : AbstractValidator<CompleteRegistrationCommand>
{
    /// <summary>Configures rules: ticket required, first/last name required, password optional but min-length when set.</summary>
    public CompleteRegistrationCommandValidator()
    {
        RuleFor(x => x.Ticket)
            .WhylandRequired()
            .WithName("تیکت ثبت‌نام");

        RuleFor(x => x.FirstName)
            .WhylandRequired()
            .WhylandMaxLen(100)
            .WithName("نام");

        RuleFor(x => x.LastName)
            .WhylandRequired()
            .WhylandMaxLen(100)
            .WithName("نام خانوادگی");

        // Password is optional (OTP-only account); but when provided it must be strong.
        RuleFor(x => x.Password)
            .MinimumLength(8)
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
            .WithName("رمز عبور");
    }
}
