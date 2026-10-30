namespace Whyland.Application.Features.Admin.Users.Commands.Create;

/// <summary>
/// Handles <see cref="CreateUserCommand"/>: keeps the phone number and e-mail unique, stores
/// only a hashed password, and assigns the selected roles.
/// </summary>
/// <param name="unitOfWork">The unit of work providing user persistence.</param>
/// <param name="passwordHasher">Hashes the initial password; it is never stored in plaintext.</param>
public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHasher) : ICommandHandler<CreateUserCommand, Guid>
{
    /// <summary>
    /// Creates the account, or returns a uniqueness error when its identifier is already taken
    /// and a generic error when persistence fails.
    /// </summary>
    /// <param name="command">The values submitted in the create modal.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The new account identifier, or an error.</returns>
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        // The stored forms are the ones every login lookup uses: one spelling per number, trimmed e-mail.
        var phone = string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? null
            : PhoneNormalizer.Normalize(command.PhoneNumber);
        var email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();

        if (phone is not null &&
            await unitOfWork.UserCommandRepository.GetTrackedByPhoneAsync(phone, ct) is not null)
            return UserErrors.PhoneTaken(phone);

        if (email is not null &&
            await unitOfWork.UserCommandRepository.GetTrackedByEmailAsync(email, ct) is not null)
            return UserErrors.EmailTaken(email);

        var user = Domain.Entities.Identity.User.Create(
            command.FirstName.Trim(),
            command.LastName.Trim(),
            phoneNumber: phone,
            email: email,
            passwordHash: passwordHasher.Hash(command.Password),
            isActive: command.IsActive);

        await unitOfWork.UserCommandRepository.AddAsync(user, ct);

        var fullName = $"{user.FirstName} {user.LastName}";
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return UserErrors.CreateFailed();

        await unitOfWork.UserCommandRepository.ReplaceRolesAsync(user.Id, command.RoleIds, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure
            ? UserErrors.CreateFailed()
            : user.Id;
    }
}
