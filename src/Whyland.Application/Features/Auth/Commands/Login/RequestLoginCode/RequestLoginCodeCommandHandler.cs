using Whyland.Application.Features.Auth.Login.RequestLoginCode;
using Whyland.Application.Features.Auth.Otp;

namespace Whyland.Application.Features.Auth.Commands.Login.RequestLoginCode;

/// <summary>
/// Handles <see cref="RequestLoginCodeCommand"/>: issues and sends a code of the
/// <see cref="OtpPurpose.Login"/> purpose (s5.1.4). The issuance rules — cooldown, hash-only
/// storage, fail-closed on a delivery failure — belong to <see cref="OtpCodeIssuer"/>.
/// </summary>
/// <param name="unitOfWork">Provides OTP persistence.</param>
/// <param name="codeGenerator">Generates the random numeric code.</param>
/// <param name="sms">Sends the code to the user.</param>
/// <param name="passwordHasher">Hashes the code so only the hash is stored.</param>
/// <param name="options">OTP configuration.</param>
public sealed class RequestLoginCodeCommandHandler(
    IUnitOfWork unitOfWork,
    ICodeGeneratorService codeGenerator,
    ISmsService sms,
    IPasswordHashService passwordHasher,
    IOptions<AuthOptions> options
) : ICommandHandler<RequestLoginCodeCommand>
{
    /// <summary>Issues and sends a login code, or returns a cooldown/validation error.</summary>
    public Task<Result> Handle(RequestLoginCodeCommand command, CancellationToken ct) =>
        OtpCodeIssuer.SendAsync(
            unitOfWork, codeGenerator, sms, passwordHasher, options,
            command.PhoneNumber, OtpPurpose.Login, ct);
}
