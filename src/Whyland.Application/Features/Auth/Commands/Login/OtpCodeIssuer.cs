using Microsoft.Extensions.Options;
using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Common.Options;
using Whyland.Application.Features.Auth.Errors;
using Whyland.Domain.Entities.Identity;

namespace Whyland.Application.Features.Auth.Otp;

/// <summary>
/// Issues and sends a one-time code of a given purpose. Both OTP paths need the identical,
/// security-critical sequence — cooldown, hash-only storage, fail-closed on a delivery failure —
/// so it lives here once (s5.1.4, s11.3.11).
/// </summary>
internal static class OtpCodeIssuer
{
    /// <summary>
    /// Sends a fresh code for <paramref name="phoneNumber"/> under <paramref name="purpose"/>,
    /// or returns the cooldown/send failure. The plaintext code is never logged or persisted.
    /// </summary>
    internal static async Task<Result> SendAsync(
        IUnitOfWork unitOfWork,
        ICodeGeneratorService codeGenerator,
        ISmsService sms,
        IPasswordHashService passwordHasher,
        IOptions<AuthOptions> options,
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken ct)
    {
        var opts = options.Value;
        var phone = PhoneNormalizer.Normalize(phoneNumber);

        // Throttle on the newest row of this purpose, used or not, so an invalidated code cannot
        // reset the cooldown — and a binding code never blocks a login code for the same number.
        var last = await unitOfWork.OtpRequestCommandRepository.GetLatestAsync(phone, purpose, ct);
        if (last is not null
            && (DateTime.UtcNow - last.CreatedAt).TotalSeconds < opts.OtpResendCooldownSeconds)
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
            Purpose = purpose,
        };

        await unitOfWork.OtpRequestCommandRepository.AddAsync(request, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return AuthErrors.LoginFailed;

        var sent = await sms.SendTemplateAsync(
            phone,
            opts.OtpSmsTemplateCode,
            new Dictionary<string, object?>
            {
                ["code"] = code,
                ["expirationMinutes"] = opts.OtpExpiryMinutes,
            },
            ct);

        if (!sent.Success)
        {
            // Fail closed: a code the user may never have received must not authenticate.
            // An indeterminate delivery can still mean the SMS arrived, but the sender cannot
            // prove it, so the row is burned and the user requests a fresh code (s11.3.11).
            request.IsUsed = true;
            await unitOfWork.SaveAsync(ct);

            return AuthErrors.SmsSendFailed;
        }

        return Result.Success();
    }
}
