using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Features.Auth.Errors;
using Whyland.Application.Features.Auth.Login.VerifyLoginCode;
using Whyland.Shared.Dtos.Identity;

namespace Whyland.Application.Features.Auth.Commands.Login.VerifyLoginCode;

/// <summary>
/// Handles <see cref="VerifyLoginCodeCommand"/>: validates the code against the stored hash,
/// enforces expiry and attempt limits, and marks the code used (s5.1.4).
/// <para>
/// The verified phone then resolves to exactly one of two outcomes:
/// <list type="bullet">
///   <item><description>An existing, active account → stamps LastLoginAt and returns the
///   <see cref="AuthenticatedUserDto"/> for the web layer to issue a cookie.</description></item>
///   <item><description>A brand-new phone → issues a single-use <see cref="IRegistrationTicketService"/>
///   ticket so the client can complete registration in a second step (s5.3).</description></item>
/// </list>
/// </para>
/// </summary>
/// <param name="unitOfWork">Provides OTP and User persistence.</param>
/// <param name="passwordHasher">Verifies the code against its stored hash.</param>
/// <param name="tickets">Issues the registration ticket for newly-seen phones.</param>
public sealed class VerifyLoginCodeCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher,
    IRegistrationTicketService tickets
) : ICommandHandler<VerifyLoginCodeCommand, VerifyLoginResultDto>
{
    /// <summary>Verifies the login code and returns either the signed-in user or a registration ticket.</summary>
    public async Task<Result<VerifyLoginResultDto>> Handle(VerifyLoginCodeCommand command, CancellationToken ct)
    {
        var phone = command.PhoneNumber.Trim();
        var otp = await unitOfWork.OtpRequestCommandRepository.GetLatestUsableAsync(phone, ct);

        // One generic failure for missing / used / expired / exhausted (anti-enumeration).
        if (otp is null || otp.IsUsed || otp.ExpiresAt <= DateTime.UtcNow)
            return AuthErrors.InvalidOtp;

        // Wrong code: burn one attempt, then persist.
        if (!passwordHasher.Verify(command.Code, otp.CodeHash))
        {
            otp.AttemptCount++;
            if (otp.AttemptCount >= otp.MaxAttempts)
                otp.IsUsed = true;

            await unitOfWork.SaveAsync(ct);
            return AuthErrors.InvalidOtp;
        }

        // Correct code: burn it so it can never be replayed.
        otp.IsUsed = true;
        await unitOfWork.SaveAsync(ct);

        var user = await unitOfWork.UserQueryRepository.GetByPhoneAsync(phone, ct);

        // Existing but deactivated account must never authenticate (s5.1.3).
        if (user is not null && !user.IsActive)
            return AuthErrors.InvalidOtp;

        // Existing account → stamp login time and return the identity for the cookie.
        if (user is not null)
        {
            unitOfWork.UserCommandRepository.TouchLastLogin(user);
            await unitOfWork.SaveAsync(ct);

            var permissions = await unitOfWork.UserQueryRepository.GetPermissionKeysAsync(user.Id, ct);
            var authenticated = new AuthenticatedUserDto(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                permissions
            );
            return new VerifyLoginResultDto(authenticated, null);
        }

        // New phone → hand back a short-lived ticket for the registration step.
        var ticket = tickets.Issue(phone);
        return new VerifyLoginResultDto(null, ticket);
    }
}
