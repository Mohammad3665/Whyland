using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Common.Validation;
using Whyland.Application.Features.Auth.Errors;
using Whyland.Domain.Entities.Identity;

namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Handles <see cref="ForgotPasswordConfirmCommand"/> (s5.4).
/// <para>
/// The code is checked first, so an account is only ever touched by someone holding a code that was
/// issued for its number. A wrong code burns one attempt and the whole row once the limit is reached;
/// a correct code is burned immediately so it can never be replayed.
/// </para>
/// <para>
/// Recovery also clears the password-login lockout: the person has just proved ownership of the
/// number, which is exactly what the lockout was protecting against.
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides the tracked user and OTP rows.</param>
/// <param name="passwordHasher">Verifies the code and hashes the new password.</param>
public sealed class ForgotPasswordConfirmCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher
) : ICommandHandler<ForgotPasswordConfirmCommand>
{
    /// <summary>Verifies the recovery code and stores the new password.</summary>
    public async Task<Result> Handle(ForgotPasswordConfirmCommand command, CancellationToken ct)
    {
        var phone = PhoneNormalizer.Normalize(command.PhoneNumber);

        var otp = await unitOfWork.OtpRequestCommandRepository.GetLatestUsableAsync(
            phone, OtpPurpose.PasswordReset, ct);

        // One generic failure for missing / used / expired (anti-enumeration).
        if (otp is null || otp.IsUsed || otp.ExpiresAt <= DateTime.UtcNow)
            return AuthErrors.InvalidOtp;

        if (!passwordHasher.Verify(command.Code, otp.CodeHash))
        {
            otp.AttemptCount++;
            if (otp.AttemptCount >= otp.MaxAttempts)
                otp.IsUsed = true;

            await unitOfWork.SaveAsync(ct);
            return AuthErrors.InvalidOtp;
        }

        otp.IsUsed = true;

        var user = await unitOfWork.UserCommandRepository.GetTrackedByPhoneAsync(phone, ct);

        // The account was removed or deactivated while the code was in flight.
        if (user is null || !user.IsActive)
        {
            await unitOfWork.SaveAsync(ct);
            return AuthErrors.InvalidOtp;
        }

        user.PasswordHash = passwordHasher.Hash(command.NewPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutUntilUtc = null;

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? AuthErrors.LoginFailed : Result.Success();
    }
}
