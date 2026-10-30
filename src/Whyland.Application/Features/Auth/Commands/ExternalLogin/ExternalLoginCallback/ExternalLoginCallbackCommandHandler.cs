namespace Whyland.Application.Features.Auth.ExternalLogin;

/// <summary>
/// Handles <see cref="ExternalLoginCallbackCommand"/> (s5.4).
/// <para>
/// A known provider id signs the linked account in. An unknown one is provisioned on the spot —
/// with no phone number, because the point of signing in with Google is not to enter one. The phone
/// is collected later, and only when the user sets a password that must stay recoverable by SMS.
/// </para>
/// </summary>
/// <param name="unitOfWork">Reads and writes the provider link, the user row and its permissions.</param>
public sealed class ExternalLoginCallbackCommandHandler(
    IUnitOfWork unitOfWork
) : ICommandHandler<ExternalLoginCallbackCommand, AuthenticatedUserDto>
{
    /// <summary>Returns the linked account, or the one just created for this provider identity.</summary>
    public async Task<Result<AuthenticatedUserDto>> Handle(ExternalLoginCallbackCommand command, CancellationToken ct)
    {
        var identity = command.Identity;
        var link = await unitOfWork.ExternalLoginCommandRepository
            .GetByProviderKeyAsync(identity.Provider, identity.ProviderKey, ct);

        if (link is null)
            return await ProvisionAsync(identity, ct);

        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(link.UserId, ct);

        // A deactivated or deleted account must not authenticate through its provider link.
        if (user is null || !user.IsActive)
            return UserErrors.InvalidCredentials();

        return await AuthenticatedAsync(user, ct);
    }

    /// <summary>
    /// Creates the account a first-time Google sign-in stands for: the names and e-mail the provider
    /// released, no phone, no password, plus the link that makes the next sign-in resolve to this row.
    /// </summary>
    private async Task<Result<AuthenticatedUserDto>> ProvisionAsync(ExternalIdentityDto identity, CancellationToken ct)
    {
        var email = string.IsNullOrWhiteSpace(identity.Email) ? null : identity.Email.Trim();

        // An e-mail claim is a hint, never a proof of ownership, so it must not link this Google
        // identity to the account that already uses it — and the partial unique index on Users.Email
        // forbids keeping the address either, so the new row is created without it.
        if (email is not null
            && await unitOfWork.UserCommandRepository.GetTrackedByEmailAsync(email, ct) is not null)
        {
            email = null;
        }

        var firstName = identity.FirstName.Trim();
        var lastName = identity.LastName.Trim();

        // Some Google accounts release only an e-mail, never a name; FirstName is required and is
        // what the header shows, so the local part of the address stands in for it.
        if (firstName.Length == 0 && email is not null)
            firstName = email.Split('@')[0];

        // No phone number and no password: this account signs in through the provider link only,
        // and a number is asked for if and when it decides to have a password (s5.4).
        var user = Domain.Entities.Identity.User.Create(
            Clip(firstName, Domain.Entities.Identity.User.MaxFirstNameLength)!, Clip(lastName, Domain.Entities.Identity.User.MaxLastNameLength)!, email: Clip(email, Domain.Entities.Identity.User.MaxEmailLength));
        user.RecordSuccessfulLogin(DateTime.UtcNow);

        await unitOfWork.UserCommandRepository.AddAsync(user, ct);
        await unitOfWork.ExternalLoginCommandRepository.AddAsync(new Domain.Entities.Identity.ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = identity.Provider,
            ProviderKey = identity.ProviderKey,
            CreatedAt = DateTime.UtcNow,
        }, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return UserErrors.LoginFailed();

        return await AuthenticatedAsync(user, ct);
    }

    /// <summary>Shapes the cookie payload for a user, including its effective permissions.</summary>
    private async Task<Result<AuthenticatedUserDto>> AuthenticatedAsync(
        Domain.Entities.Identity.User user,
        CancellationToken ct)
    {
        var permissions = await unitOfWork.UserQueryRepository.GetPermissionKeysAsync(user.Id, ct);

        return new AuthenticatedUserDto(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            permissions);
    }

    /// <summary>Cuts a provider-supplied value to the column length instead of failing the insert.</summary>
    private static string? Clip(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
