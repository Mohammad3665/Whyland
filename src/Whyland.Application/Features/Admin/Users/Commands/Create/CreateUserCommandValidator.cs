using Microsoft.Extensions.Options;
using Whyland.Application.Common.Options;
using Whyland.Application.Common.Validation;

namespace Whyland.Application.Features.Admin.Users.Commands.Create;

/// <summary>
/// Validates the create-user modal: both names are mandatory, the optional identifier fields
/// must be well-formed, and the initial password must satisfy the configured strength rules.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>Configures the validation rules for every new account field.</summary>
    public CreateUserCommandValidator(IOptions<AuthOptions> options)
    {
        RuleFor(x => x.FirstName)
            .WhylandRequired()
            .WhylandMaxLen(100)
            .WithName("نام");

        RuleFor(x => x.LastName)
            .WhylandRequired()
            .WhylandMaxLen(100)
            .WithName("نام خانوادگی");

        RuleFor(x => x.Password)
            .WhylandRequired()
            .WhylandStrongPassword(options.Value.PasswordMinLength)
            .WithName("رمز عبور");

        RuleFor(x => x.PhoneNumber)
            // The shape check runs on Latin digits, so a number typed in Persian is accepted
            // exactly like the same number typed in English; the handler stores the normalized form.
            .Must(PhoneNormalizer.HasValidShape)
                .WithMessage("شماره موبایل معتبر وارد کنید.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithName("شماره موبایل");

        RuleFor(x => x.Email)
            .EmailAddress()
                .WithMessage("ایمیل معتبر وارد کنید.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithName("ایمیل");
    }
}
