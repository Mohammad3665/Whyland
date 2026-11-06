namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.SavePermissions;

/// <summary>
/// Handles <see cref="SaveRolePermissionsCommand"/>: verifies that every identifier really is a
/// permission, then rewrites the role's grant set and persists it through the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing role persistence.</param>
public sealed class SaveRolePermissionsCommandHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<SaveRolePermissionsCommand>
{
    /// <summary>
    /// Saves the grant set, or returns an error when the role does not exist, is a system role, or one
    /// of the identifiers belongs to no permission. A persistence failure is masked.
    /// </summary>
    /// <param name="command">The identifiers checked in the permission tree.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(SaveRolePermissionsCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.RoleCommandRepository;

        var role = await repository.GetByIdAsync(command.RoleId, ct);
        if (role is null)
            return RoleErrors.NotFound(command.RoleId);

        // What a system role grants is decided by its seed row and by the code that depends on it, so
        // the tree is read-only for those rows; anything else would let one save strip the super
        // admin of the very permission the panel is open with.
        if (role.IsSystem)
            return RoleErrors.SystemRoleProtected();

        // A key that does not exist cannot be honoured by anything, and the effective-permission read
        // would not show it either — so the stale id is refused here and named in the error, rather
        // than stored as a grant that silently does nothing.
        var unknown = await repository.GetUnknownPermissionIdsAsync(command.PermissionIds, ct);
        if (unknown.Count > 0)
            return RoleErrors.PermissionNotFound(unknown.First());

        await repository.ReplacePermissionsAsync(command.RoleId, command.PermissionIds, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? RoleErrors.UpdateFailed() : Result.Success();
    }
}
