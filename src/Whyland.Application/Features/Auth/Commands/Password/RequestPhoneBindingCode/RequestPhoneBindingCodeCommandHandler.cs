using Whyland.Application.Features.Auth.Otp;

namespace Whyland.Application.Features.Auth.Password.RequestPhoneBindingCode;

/// <summary>
/// Handles <see cref="RequestPhoneBindingCodeCommand"/> (s5.4): sends a code of the
/// <see cref="OtpPurpose.PhoneBinding"/> purpose, which a login code can never be mistaken for.
/// Only an account that still has no number may request one, and only for a number that no other
/// account owns — the partial unique index on Users.PhoneNumber makes a second row impossible.
/// </summary>
/// <param name="unitOfWork">Reads the account and the other phone numbers.</param>
/// <param name="codeGenerator">Generates the random numeric code.</param>
/// <param name="sms">Sends the code to the user.</param>
/// <param name="passwordHasher">Hashes the code so only the hash is stored.</param>
/// <param name="options">OTP configuration.</param>
public sealed class RequestPhoneBindingCodeCommandHandler(
    IUnitOfWork unitOfWork,
    ICodeGeneratorService codeGenerator,
    ISmsService sms,
    IPasswordHashService passwordHasher,
    IOptions<AuthOptions> options
) : ICommandHandler<RequestPhoneBindingCodeCommand>
{
    /// <summary>Issues and sends the binding code, or explains why it cannot be sent.</summary>
    public async Task<Result> Handle(RequestPhoneBindingCodeCommand command, CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.UserId, ct);
        if (user is null)
            return UserErrors.InvalidCredentials();

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            return UserErrors.PhoneAlreadySet();

        var phone = PhoneNormalizer.Normalize(command.PhoneNumber);

        var owner = await unitOfWork.UserQueryRepository.GetByPhoneAsync(phone, ct);
        if (owner is not null)
            return UserErrors.PhoneTaken(phone);

        return await OtpCodeIssuer.SendAsync(
            unitOfWork, codeGenerator, sms, passwordHasher, options,
            phone, OtpPurpose.PhoneBinding, ct);
    }
}
