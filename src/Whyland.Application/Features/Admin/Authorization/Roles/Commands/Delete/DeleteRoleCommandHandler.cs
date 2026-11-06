namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteRoleCommand"/> by clearing the role's joins and soft-deleting the row in a
/// single unit of work, so no save can land between the two and leave a hidden role still granting.
/// </summary>
/// <param name="unitOfWork">The unit of work providing role persistence.</param>
public sealed class DeleteRoleCommandHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteRoleCommand>
{
    /// <summary>
    /// Soft-deletes the requested role, or returns an error when it does not exist, is a system role,
    /// or persistence fails.
    /// </summary>
    /// <param name="command">The command containing the role identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.RoleCommandRepository;

        var role = await repository.GetByIdAsync(command.Id, ct);
        if (role is null)
            return RoleErrors.NotFound(command.Id);

        // The roles the application depends on are removed by editing their seed row, never from the
        // panel: deleting SuperAdmin would lock every operator out with no way back in.
        if (role.IsSystem)
            return RoleErrors.SystemRoleProtected();

        // Both joins go before the flag, in the same transaction as the delete itself: a role that is
        // detached but not yet deleted, or deleted but still attached, is the same broken state seen
        // from either side.
        await repository.DetachUsersAsync(command.Id, ct);
        await repository.ReplacePermissionsAsync(command.Id, [], ct);

        repository.SoftDelete(role);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? RoleErrors.SoftDeleteFailed() : Result.Success();
    }
}
