namespace Whyland.Application.Features.Admin.Users.Commands.Update;

/// <summary>
/// Validates the edit-user modal; the same field rules as creation, without a password.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    /// <summary>Configures the validation rules for every editable account field.</summary>
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .WhylandRequired()
            .WhylandMaxLen(User.MaxFirstNameLength)
            .WithName("نام");

        RuleFor(x => x.LastName)
            .WhylandRequired()
            .WhylandMaxLen(User.MaxLastNameLength)
            .WithName("نام خانوادگی");

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
