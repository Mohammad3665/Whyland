using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Domain.Model.Authorization.Role.Repositories;

/// <summary>
/// Command-side repository for the Role aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IRoleCommandRepository
{
    #region Reads

    /// <summary>
    /// Returns the tracked role, or null when there is none with that identifier. Soft-deleted rows
    /// are already hidden by the global query filter, so a deleted role reads as missing.
    /// </summary>
    /// <param name="id">Role identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<RoleEntity?> GetByIdAsync(RoleId id, CancellationToken ct = default);

    /// <summary>
    /// Whether another live role already uses this name. A role name is unique across the whole
    /// table — unlike a category name, which only has to be unique inside its sibling group — because
    /// the code and the seeders look roles up by this exact string.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <param name="excludeId">A row that must not count against itself, when editing.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsNameTakenAsync(string name, RoleId? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// The identifiers that do not resolve to a live permission. A grant carrying a key that does not
    /// exist would be silently invisible in <c>GetPermissionKeysAsync</c>, so the write path checks
    /// them before staging anything — and reports the offending ones, because the operator has to be
    /// told which box of the tree was stale rather than that some box was.
    /// </summary>
    /// <param name="permissionIds">The identifiers to resolve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identifiers with no permission behind them; empty when the whole set is valid.</returns>
    Task<IReadOnlyCollection<PermissionId>> GetUnknownPermissionIdsAsync(
        IReadOnlyCollection<PermissionId> permissionIds,
        CancellationToken ct = default);

    #endregion

    #region Writes

    /// <summary>Stages a new role; persisted on SaveAsync.</summary>
    /// <param name="role">The role to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(RoleEntity role, CancellationToken ct = default);

    /// <summary>
    /// Marks the role as deleted (hidden by the global query filter). The row is kept for the audit
    /// trail; the caller must also clear its grants and detach it from its users, because those join
    /// tables are not filtered and a hidden role would otherwise keep granting permissions.
    /// </summary>
    /// <param name="role">The tracked role to soft-delete.</param>
    void SoftDelete(RoleEntity role);

    /// <summary>
    /// Makes the role's grant set exactly the given permission identifiers: rows missing from the
    /// list are removed and identifiers not yet granted are added, so re-saving the same set is a
    /// no-op instead of a delete/re-insert churn. Passing an empty collection revokes everything,
    /// which is how a delete clears the role. Staged only; persisted on SaveAsync.
    /// </summary>
    /// <param name="roleId">The role whose grants are being rewritten.</param>
    /// <param name="permissionIds">The full set of permissions the role should end up with.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReplacePermissionsAsync(
        RoleId roleId,
        IReadOnlyCollection<PermissionId> permissionIds,
        CancellationToken ct = default);

    /// <summary>
    /// Removes the role from every user that currently holds it. A deleted role left attached would
    /// keep showing in the user's role list and, worse, its name would still resolve in
    /// <c>IsInRoleAsync</c> checks. Staged only; persisted on SaveAsync.
    /// </summary>
    /// <param name="roleId">The role being detached.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many assignments were removed.</returns>
    Task<int> DetachUsersAsync(RoleId roleId, CancellationToken ct = default);

    #endregion
}
