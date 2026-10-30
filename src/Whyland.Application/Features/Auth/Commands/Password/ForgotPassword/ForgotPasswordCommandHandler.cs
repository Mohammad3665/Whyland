using Whyland.Application.Features.Auth.Otp;

namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Handles <see cref="ForgotPasswordCommand"/> (s5.4): sends a six-digit code instead of a reset link.
/// <para>
/// The code is issued under <see cref="OtpPurpose.PasswordReset"/>, so it can neither sign anyone in
/// nor be confused with a login or phone-binding code. A number that matches no active account gets
/// the same neutral answer as a delivered code, which keeps this endpoint unusable for enumeration.
/// </para>
/// <para>
/// A delivery failure also answers neutrally: the code row is already burned by the issuer (fail
/// closed), so the caller simply requests a fresh one after the cooldown. The code itself never
/// reaches the log.
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides the tracked user row and OTP persistence.</param>
/// <param name="codeGenerator">Generates the random numeric code.</param>
/// <param name="sms">Delivers the code to the account's number.</param>
/// <param name="passwordHasher">Hashes the code so only the hash is stored.</param>
/// <param name="options">OTP lifetime, cooldown and template.</param>
/// <param name="logger">Diagnostics that never contain the code.</param>
public sealed class ForgotPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    ICodeGeneratorService codeGenerator,
    ISmsService sms,
    IPasswordHashService passwordHasher,
    IOptions<AuthOptions> options,
    ILogger<ForgotPasswordCommandHandler> logger
) : ICommandHandler<ForgotPasswordCommand>
{
    /// <summary>Issues and delivers the recovery code, always reporting success to the caller.</summary>
    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var phone = PhoneNormalizer.Normalize(command.PhoneNumber);

        var user = await unitOfWork.UserCommandRepository.GetTrackedByPhoneAsync(phone, ct);

        // Unknown number or deactivated account: the caller still sees the neutral answer,
        // and no message is sent to a number nobody owns.
        if (user is null || !user.IsActive)
            return Result.Success();

        var sent = await OtpCodeIssuer.SendAsync(
            unitOfWork, codeGenerator, sms, passwordHasher, options,
            phone, OtpPurpose.PasswordReset, ct);

        if (sent.IsFailure)
            logger.LogWarning(
                "Password reset code was not issued for user {UserId} ({ErrorCode})",
                user.Id,
                sent.Error.Code);

        // Cooldown or a failed send changes nothing for the caller (s15.1.3).
        return Result.Success();
    }
}
