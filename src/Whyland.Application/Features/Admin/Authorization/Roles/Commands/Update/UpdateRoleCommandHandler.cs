namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateRoleCommand"/>: re-checks the name against the other roles, then lets the
/// tracked entity apply its own invariants and persists the change through the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing role persistence.</param>
public sealed class UpdateRoleCommandHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRoleCommand>
{
    /// <summary>
    /// Updates the role, or returns an error when it does not exist, is a system role, or the new
    /// name belongs to another role. Persistence failures are masked behind a generic message.
    /// </summary>
    /// <param name="command">The values submitted in the edit modal.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.RoleCommandRepository;

        var role = await repository.GetByIdAsync(command.Id, ct);
        if (role is null)
            return RoleErrors.NotFound(command.Id);

        // Read-only before anything is compared: a system role keeps its seed identity, and its name
        // is the string the seeder looks up, so even an unchanged save is refused.
        if (role.IsSystem)
            return RoleErrors.SystemRoleProtected();

        var name = command.Name.Trim();
        var title = command.Title.Trim();

        // The row excludes itself: re-typing the same name in the edit modal is not a clash.
        if (await repository.IsNameTakenAsync(name, command.Id, ct))
            return RoleErrors.NameTaken(name);

        var update = role.Update(name, title);
        if (update.IsFailure)
            return update.Error;

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? RoleErrors.UpdateFailed() : Result.Success();
    }
}
