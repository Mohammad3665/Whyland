using Whyland.Domain.Model.Identity.InstructorProfile.Entities;
using Whyland.Domain.Common.Errors;

namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Update;

/// <summary>Validates the edit form against the profile constraints.</summary>
public sealed class UpdateInstructorProfileCommandValidator : AbstractValidator<UpdateInstructorProfileCommand>
{
    public UpdateInstructorProfileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WhylandWithError(ValidationErrors.Required)
            .WithName("شناسه");

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
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(UpdateInstructorProfileCommand.Website)))
            .WithName("وب‌سایت");

        RuleFor(x => x.LinkedIn)
            .MaximumLength(InstructorProfile.MaxLinkedInLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxLinkedInLength))
            .Must(InstructorProfileInputNormalizer.IsHttpUrl)
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(UpdateInstructorProfileCommand.LinkedIn)))
            .WithName("لینکدین");

        RuleFor(x => x.Instagram)
            .MaximumLength(InstructorProfile.MaxInstagramLength)
            .WhylandWithError(ValidationErrors.TooLong, ("Max", InstructorProfile.MaxInstagramLength))
            .Must(InstructorProfileInputNormalizer.IsHttpUrl)
            .WhylandWithError(InstructorProfileErrors.InvalidSocialUrl(nameof(UpdateInstructorProfileCommand.Instagram)))
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
