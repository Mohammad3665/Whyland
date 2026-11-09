namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Create;

/// <summary>
/// Handles <see cref="CreateRoleCommand"/>: keeps the role name unique, builds the row through the
/// entity's own factory and persists it through the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing role persistence.</param>
public sealed class CreateRoleCommandHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<CreateRoleCommand, RoleId>
{
    /// <summary>
    /// Creates the role, or returns a uniqueness error when its name is already taken and a generic
    /// error when persistence fails.
    /// </summary>
    /// <param name="command">The values submitted in the create modal.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The new role identifier, or an error.</returns>
    public async Task<Result<RoleId>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.RoleCommandRepository;

        // The stored forms are the ones every lookup uses: the trimmed name is what the seeders and
        // the code compare against, so it must not be saved with the padding the request carried.
        var name = command.Name.Trim();
        var title = command.Title.Trim();

        // Uniqueness is a property of the store, not of the entity: two roles can each be perfectly
        // valid on their own and still break the system by sharing a name.
        if (await repository.IsNameTakenAsync(name, null, ct))
            return RoleErrors.NameTaken(name);

        var created = Role.Create(name, title, isSystem: false);
        if (created.IsFailure)
            return created.Error;

        var role = created.Data!;
        await repository.AddAsync(role, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return RoleErrors.CreateFailed();

        return role.Id;
    }
}
