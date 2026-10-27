using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Features.Auth.Errors;

namespace Whyland.Application.Features.Auth.Password.ChangePassword;

/// <summary>
/// Handles <see cref="ChangePasswordCommand"/> (s13.2.3): replaces a password that already exists.
/// <para>
/// The old password must be proved, since this form is the way to replace a password the user still
/// knows. An account that has none is sent to the set-password flow instead, and the phone number is
/// never touched here: it is the recovery channel, and it gets bound while the first password is
/// created (s5.4).
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides the tracked user row.</param>
/// <param name="passwordHasher">Hashes the new password and verifies the old one.</param>
public sealed class ChangePasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher
) : ICommandHandler<ChangePasswordCommand>
{
    /// <summary>Verifies the stored password and replaces it with the new one.</summary>
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.UserId, ct);
        if (user is null)
            return AuthErrors.InvalidCredentials;

        // Nothing to prove and nothing to replace: this account belongs to the other flow.
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return AuthErrors.PasswordNotSet;

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            return AuthErrors.WrongPassword;

        user.PasswordHash = passwordHasher.Hash(command.NewPassword);

        // A password change clears any lockout the old password may have accumulated (s15.1.4).
        user.FailedLoginAttempts = 0;
        user.LockoutUntilUtc = null;

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? AuthErrors.LoginFailed : Result.Success();
    }
}
