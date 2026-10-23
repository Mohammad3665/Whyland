using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Features.Auth.Errors;
using Whyland.Application.Features.Auth.Registration.CompleteRegistration;
using Whyland.Shared.Dtos.Identity;

namespace Whyland.Application.Features.Auth.Commands.Registration.CompleteRegistration;

/// <summary>
/// Handles <see cref="CompleteRegistrationCommand"/>: consumes the single-use registration
/// ticket (proving the phone was verified), creates the account, optionally stores a hashed
/// password, and returns the identity for the web layer to issue a cookie (s5.3, s5.1.1).
/// </summary>
/// <param name="unitOfWork">Provides User persistence.</param>
/// <param name="passwordHasher">Hashes the optional password (never stored in plaintext).</param>
/// <param name="tickets">Consumes the single-use registration ticket.</param>
public sealed class CompleteRegistrationCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher,
    IRegistrationTicketService tickets
) : ICommandHandler<CompleteRegistrationCommand, AuthenticatedUserDto>
{
    /// <summary>Creates the account from a valid ticket and returns the authenticated user, or an error.</summary>
    public async Task<Result<AuthenticatedUserDto>> Handle(CompleteRegistrationCommand command, CancellationToken ct)
    {
        // The ticket proves the phone was verified in the prior step; it is burned here.
        var phone = tickets.Consume(command.Ticket);
        if (phone is null)
            return AuthErrors.InvalidTicket;

        // Guard against a race: the account may have been created since the ticket was issued.
        var existing = await unitOfWork.UserQueryRepository.GetByPhoneAsync(phone, ct);
        if (existing is not null)
            return AuthErrors.RegistrationConflict;

        var user = new Domain.Entities.Identity.User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phone,
            // UserName is unique; the verified phone is a safe, collision-free value.
            UserName = phone,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            // A password is stored only when the user chose one; otherwise OTP-only (s5.1.1).
            PasswordHash = string.IsNullOrWhiteSpace(command.Password)
                ? null
                : passwordHasher.Hash(command.Password),
            IsActive = true,
            LastLoginAt = DateTime.UtcNow,
        };

        await unitOfWork.UserCommandRepository.AddAsync(user, ct);
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return AuthErrors.LoginFailed;

        // A freshly-registered public user holds no admin permissions; the cookie is still issued.
        var permissions = await unitOfWork.UserQueryRepository.GetPermissionKeysAsync(user.Id, ct);

        return new AuthenticatedUserDto(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            permissions
        );
    }
}
