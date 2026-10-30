namespace Whyland.Application.Features.Admin.Users.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateUserCommand"/> by rewriting the account's identity fields and
/// replacing its role assignments.
/// </summary>
/// <param name="unitOfWork">The unit of work providing user persistence.</param>
public sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    /// <summary>
    /// Applies the new values, rejecting a conflict with another account's phone number or
    /// e-mail and reporting a missing account as not found.
    /// </summary>
    /// <param name="command">The values submitted in the edit modal.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.Id, ct);
        if (user is null)
            return UserErrors.NotFound(command.Id);

        var phone = string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? null
            : PhoneNormalizer.Normalize(command.PhoneNumber);
        var email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();

        // A number already owned by somebody else would silently move that account's sign-in
        // and recovery channel, so it is refused instead of reassigned.
        var phoneOwner = phone is null
            ? null
            : await unitOfWork.UserCommandRepository.GetTrackedByPhoneAsync(phone, ct);
        if (phoneOwner is not null && phoneOwner.Id != user.Id)
            return UserErrors.PhoneTaken(phone!);

        var emailOwner = email is null
            ? null
            : await unitOfWork.UserCommandRepository.GetTrackedByEmailAsync(email, ct);
        if (emailOwner is not null && emailOwner.Id != user.Id)
            return UserErrors.EmailTaken(email!);

        user.UpdateProfile(
            command.FirstName.Trim(), command.LastName.Trim(), phone, email, command.IsActive);

        await unitOfWork.UserCommandRepository.ReplaceRolesAsync(user.Id, command.RoleIds, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure
            ? UserErrors.UpdateFailed()
            : Result.Success();
    }
}
