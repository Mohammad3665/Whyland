using Whyland.Domain.Model.Authorization.Common.Constants;
using Whyland.Domain.Model.Authorization.Permission.Entities;
using Whyland.Domain.Model.Authorization.Role.Entities;
using Whyland.Domain.Model.Authorization.RolePermission.Entities;
using Whyland.Domain.Model.Authorization.UserRole.Entities;

namespace Whyland.Application.Features.Admin.Users.Commands.ToggleStatus;

/// <summary>
/// Handles <see cref="ToggleUserStatusCommand"/>.
/// <para>
/// Disabling is the supported way to withdraw an account: the User aggregate is intentionally
/// not soft-deletable, so its rows (orders, enrollments, logs) must stay joinable.
/// </para>
/// </summary>
/// <param name="unitOfWork">The unit of work providing user persistence.</param>
public sealed class ToggleUserStatusCommandHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<ToggleUserStatusCommand, bool>
{
    /// <summary>
    /// Flips <c>IsActive</c> and returns the new state, refusing to disable the seeded
    /// super admin — the only account that can always reach the panel.
    /// </summary>
    /// <param name="command">The command naming the account.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The state the account is in now, or an error.</returns>
    public async Task<Result<bool>> Handle(ToggleUserStatusCommand command, CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.Id, ct);
        if (user is null)
            return UserErrors.NotFound(command.Id);

        if (user.IsActive &&
            await unitOfWork.UserCommandRepository.IsInRoleAsync(user.Id.Value, RoleNames.SuperAdmin, ct))
            return UserErrors.SuperAdminProtected();

        user.SetActive(!user.IsActive);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure
            ? UserErrors.UpdateFailed()
            : user.IsActive;
    }
}
