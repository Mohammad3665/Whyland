using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.Roles;

/// <summary>
/// The EF Core command repository for the <c>Role</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work. Identifiers arrive
/// as their strongly-typed keys, so every comparison below stays between identifiers.
/// </summary>
public class RoleCommandRepository(WhylandDbContext context) : IRoleCommandRepository
{
    #region Reads

    public Task<RoleEntity?> GetByIdAsync(RoleId id, CancellationToken ct = default)
    {
        // No "not deleted" predicate: Role is ISoftDeletable, so the global query filter applied in
        // WhylandDbContext.OnModelCreating already hides the row and a delete reads as missing.
        return context.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<bool> IsNameTakenAsync(string name, RoleId? excludeId = null, CancellationToken ct = default)
    {
        var excluded = excludeId is { } skip ? skip : (RoleId?)null;

        return context.Roles.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excluded == null || x.Id != excluded), ct);
    }

    public async Task<IReadOnlyCollection<PermissionId>> GetUnknownPermissionIdsAsync(
        IReadOnlyCollection<PermissionId> permissionIds,
        CancellationToken ct = default)
    {
        if (permissionIds.Count == 0)
            return [];

        var wanted = permissionIds.ToHashSet();

        // The catalogue is a few dozen rows and this runs on a write, so the membership test is done
        // in memory rather than as a translated Contains over wrapped identifiers.
        var existing = await context.Permissions.AsNoTracking().Select(x => x.Id).ToListAsync(ct);

        return wanted.Except(existing).ToList();
    }

    #endregion

    #region Writes

    public async Task AddAsync(RoleEntity role, CancellationToken ct = default)
        => await context.Roles.AddAsync(role, ct);

    public void SoftDelete(RoleEntity role) => role.MarkDeleted();

    public async Task ReplacePermissionsAsync(
        RoleId roleId,
        IReadOnlyCollection<PermissionId> permissionIds,
        CancellationToken ct = default)
    {
        var wanted = permissionIds.ToHashSet();

        var current = await context.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(ct);

        context.RolePermissions.RemoveRange(current.Where(x => !wanted.Contains(x.PermissionId)));

        // Only the grants the role does not hold yet are inserted; re-saving an unchanged set has to
        // leave the rows alone, or every save would rewrite the whole permission history.
        var alreadyGranted = current.Select(x => x.PermissionId).ToHashSet();
        foreach (var permissionId in wanted.Where(x => !alreadyGranted.Contains(x)))
        {
            context.RolePermissions.Add(RolePermission.Create(roleId, permissionId));
        }
    }

    public async Task<int> DetachUsersAsync(RoleId roleId, CancellationToken ct = default)
    {
        var current = await context.UserRoles
            .Where(x => x.RoleId == roleId)
            .ToListAsync(ct);

        context.UserRoles.RemoveRange(current);

        return current.Count;
    }

    #endregion
}
