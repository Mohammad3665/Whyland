using Microsoft.Extensions.Options;
using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Common.Options;
using Whyland.Application.Features.Auth.Errors;
using Whyland.Application.Features.Auth.Login.RequestLoginCode;
using Whyland.Domain.Entities.Identity;

namespace Whyland.Application.Features.Auth.Commands.Login.RequestLoginCode;

/// <summary>
/// Handles <see cref="RequestLoginCodeCommand"/>: normalizes the phone, enforces a resend
/// cooldown, issues a cryptographically-random code, stores only its hash, and hands the
/// plaintext to <see cref="ISmsService"/> (s5.1.4).
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
    public async Task<Result> Handle(RequestLoginCodeCommand command, CancellationToken ct)
    {
        var opts = options.Value;
        var phone = Normalize(command.PhoneNumber);
        var last = await unitOfWork.OtpRequestCommandRepository.GetLatestUsableAsync(phone, ct);

        if (last is not null && last.ExpiresAt > DateTime.UtcNow &&
            (DateTime.UtcNow - last.CreatedAt).TotalSeconds < opts.OtpResendCooldownSeconds)
        {
            return AuthErrors.OtpCooldown;
        }

        var code = codeGenerator.Generate();
        var request = new OtpRequest
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phone,
            CodeHash = passwordHasher.Hash(code),
            AttemptCount = 0,
            MaxAttempts = opts.OtpMaxAttempts,
            ExpiresAt = DateTime.UtcNow.AddMinutes(opts.OtpExpiryMinutes),
            IsUsed = false,
        };

        await unitOfWork.OtpRequestCommandRepository.AddAsync(request, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return AuthErrors.LoginFailed;

        await sms.SendAsync(phone, $"کد ورود شما: {code}", ct);

        return Result.Success();
    }

    /// <summary>Normalizes Persian/international formats to 09xxxxxxxxx.</summary>
    private static string Normalize(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("98")) return "0" + digits[2..];
        if (digits.StartsWith("0098")) return "0" + digits[4..];
        return digits;
    }
}
