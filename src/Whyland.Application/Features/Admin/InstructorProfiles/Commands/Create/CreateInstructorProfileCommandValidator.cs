using Whyland.Domain.Common.Errors;
using Whyland.Domain.Model.Identity.InstructorProfile.Entities;

namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Create;

/// <summary>Validates the create form against the profile constraints.</summary>
public sealed class CreateInstructorProfileCommandValidator : AbstractValidator<CreateInstructorProfileCommand>
{
    public CreateInstructorProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WhylandWithError(ValidationErrors.Required)
            .WithName("کاربر");

        RuleFor(x => x.Bio)
            .MaximumLength(InstructorProfile.MaxBioLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxBioLength))
            .WithName("بیوگرافی کوتاه");

        RuleFor(x => x.Biography)
            .MaximumLength(InstructorProfile.MaxBiographyLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxBiographyLength))
            .WithName("بیوگرافی کامل");

        RuleFor(x => x.Specialization)
            .MaximumLength(InstructorProfile.MaxSpecializationLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxSpecializationLength))
            .WithName("تخصص");

        RuleFor(x => x.Website)
            .MaximumLength(InstructorProfile.MaxWebsiteLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxWebsiteLength))
            .Must(InstructorProfileInputNormalizer.IsHttpUrl)
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(CreateInstructorProfileCommand.Website)))
            .WithName("وب‌سایت");

        RuleFor(x => x.LinkedIn)
            .MaximumLength(InstructorProfile.MaxLinkedInLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxLinkedInLength))
            .Must(InstructorProfileInputNormalizer.IsHttpUrl)
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(CreateInstructorProfileCommand.LinkedIn)))
            .WithName("لینکدین");

        RuleFor(x => x.Instagram)
            .MaximumLength(InstructorProfile.MaxInstagramLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxInstagramLength))
            .Must(InstructorProfileInputNormalizer.IsHttpUrl)
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(CreateInstructorProfileCommand.Instagram)))
            .WithName("اینستاگرام");

        RuleFor(x => x.ShebaNumber)
            .Must(value => InstructorProfileInputNormalizer.IsDigitsOfLength(value, 24, stripIrPrefix: true))
            .WhylandWithError(InstructorProfileErrors.InvalidShebaNumber())
            .WithName("شماره شبا");

        RuleFor(x => x.BankAccountNumber)
            .Must(value => InstructorProfileInputNormalizer.IsDigitsBetween(value, 6, 20))
            .WhylandWithError(InstructorProfileErrors.InvalidBankAccountNumber())
            .WithName("شماره حساب");
    }
}
