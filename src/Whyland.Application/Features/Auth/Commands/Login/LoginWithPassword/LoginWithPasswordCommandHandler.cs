namespace Whyland.Application.Features.Auth.Login.LoginWithPassword;

/// <summary>
/// Handles <see cref="LoginWithPasswordCommand"/> (s5.1.1, s15.1.4).
/// <para>
/// Unknown number, an account without a password and a wrong password all produce the same
/// <see cref="UserErrors.InvalidCredentials()"/> answer, so the endpoint cannot enumerate accounts.
/// Repeated failures accumulate on the user row and temporarily block password login; OTP and
/// external login keep their own throttling and stay available.
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides the tracked user row and the permission read.</param>
/// <param name="passwordHasher">Verifies the candidate password against the stored hash.</param>
/// <param name="options">Lockout thresholds.</param>
public sealed class LoginWithPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher,
    IOptions<AuthOptions> options
) : ICommandHandler<LoginWithPasswordCommand, AuthenticatedUserDto>
{
    /// <summary>Verifies the password and returns the identity for the auth cookie, or a generic failure.</summary>
    public async Task<Result<AuthenticatedUserDto>> Handle(LoginWithPasswordCommand command, CancellationToken ct)
    {
        var opts = options.Value;
        var phoneNumber = PhoneNormalizer.Normalize(command.PhoneNumber);

        var user = await unitOfWork.UserCommandRepository.GetTrackedByPhoneAsync(phoneNumber, ct);

        // Accounts created through OTP or Google have no hash; they must use those flows.
        if (user is null || user.PasswordHash is null)
            return UserErrors.InvalidCredentials();

        var locked = user.IsLockedOut(DateTime.UtcNow);

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            user.RecordFailedAttempt(opts.PasswordMaxFailedAttempts, opts.PasswordLockoutMinutes, DateTime.UtcNow);

            await unitOfWork.SaveAsync(ct);
            return UserErrors.InvalidCredentials();
        }

        // Only revealed to someone who just proved the password, so it leaks nothing new.
        if (locked || !user.IsActive)
            return locked ? UserErrors.AccountLockedOut() : UserErrors.InvalidCredentials();

        unitOfWork.UserCommandRepository.TouchLastLogin(user);
        await unitOfWork.SaveAsync(ct);

        var permissions = await unitOfWork.UserQueryRepository.GetPermissionKeysAsync(user.Id, ct);

        return new AuthenticatedUserDto(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            permissions);
    }
}
