namespace Whyland.Application.Features.Auth.Password.SetPassword;

/// <summary>
/// Handles <see cref="SetPasswordCommand"/> (s13.2.3): gives an account its first password.
/// <para>
/// The request is already authenticated, so there is no old password to prove — instead the account
/// must not have one at all. Replacing an existing password is the job of
/// <c>ChangePasswordCommand</c>; the two responsibilities stay separate.
/// </para>
/// <para>
/// A Google-provisioned account has no phone number, and one is required before a password exists:
/// without a verified number a forgotten password could never be recovered. The number is bound with
/// a code of the <see cref="OtpPurpose.PhoneBinding"/> purpose, so a login code cannot be replayed
/// here (s5.4).
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides the tracked user row and the OTP lookups.</param>
/// <param name="passwordHasher">Hashes the new password and verifies the SMS code.</param>
public sealed class SetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher
) : ICommandHandler<SetPasswordCommand>
{
    /// <summary>Binds the missing phone number and stores the first password hash.</summary>
    public async Task<Result> Handle(SetPasswordCommand command, CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.UserId, ct);
        if (user is null)
            return UserErrors.InvalidCredentials();

        // Wrong page for an account that already has a password: that form demands the old one.
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            return UserErrors.AlreadyHasPassword();

        var bindResult = await BindPhoneAsync(user, command, ct);
        if (bindResult.IsFailure)
            return bindResult;

        // A first password cannot have accumulated a lockout, but clearing it keeps the write honest.
        user.SetPassword(passwordHasher.Hash(command.NewPassword));

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? UserErrors.LoginFailed() : Result.Success();
    }

    /// <summary>
    /// Attaches and marks as verified the phone number the account is missing. A number that is
    /// already bound is left alone: it can never be swapped through this form, precisely so it cannot
    /// silently become the recovery channel of a different account.
    /// </summary>
    private async Task<Result> BindPhoneAsync(User user, SetPasswordCommand command, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            return Result.Success();

        if (string.IsNullOrWhiteSpace(command.PhoneNumber) || string.IsNullOrWhiteSpace(command.Code))
            return UserErrors.PhoneRequired();

        var phone = PhoneNormalizer.Normalize(command.PhoneNumber);
        var otp = await unitOfWork.OtpRequestCommandRepository.GetLatestUsableAsync(
            phone, OtpPurpose.PhoneBinding, ct);

        // One generic failure for missing / used / expired / exhausted, as on the login step.
        if (otp is null || otp.IsUsed || otp.ExpiresAt <= DateTime.UtcNow)
            return UserErrors.InvalidOtp();

        if (!passwordHasher.Verify(command.Code, otp.CodeHash))
        {
            otp.AttemptCount++;
            if (otp.AttemptCount >= otp.MaxAttempts)
                otp.IsUsed = true;

            await unitOfWork.SaveAsync(ct);
            return UserErrors.InvalidOtp();
        }

        var owner = await unitOfWork.UserQueryRepository.GetByPhoneAsync(phone, ct);
        if (owner is not null && owner.Id != user.Id)
            return UserErrors.PhoneTaken(phone);

        // Correct code: burn it so it can never be replayed, then store the verified number.
        otp.IsUsed = true;
        user.BindPhoneNumber(phone);

        return Result.Success();
    }
}
