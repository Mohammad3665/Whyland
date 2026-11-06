using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.Roles;

/// <summary>
/// The EF Core command repository for the <c>Role</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
/// <remarks>
/// A wrapped key is built from the raw number the request carries at every entry point here, exactly
/// once per method, so the queries below only ever compare identifiers.
/// </remarks>
public class RoleCommandRepository(WhylandDbContext context) : IRoleCommandRepository
{
    #region Reads

    public Task<RoleEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var key = RoleId.From(id);

        // No "not deleted" predicate: Role is ISoftDeletable, so the global query filter applied in
        // WhylandDbContext.OnModelCreating already hides the row and a delete reads as missing.
        return context.Roles.FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    public Task<bool> IsNameTakenAsync(string name, long? excludeId = null, CancellationToken ct = default)
    {
        var excluded = excludeId is { } skip ? RoleId.From(skip) : (RoleId?)null;

        return context.Roles.AsNoTracking()
            .AnyAsync(x => x.Name == name && (excluded == null || x.Id != excluded), ct);
    }

    public async Task<IReadOnlyCollection<long>> GetUnknownPermissionIdsAsync(
        IReadOnlyCollection<long> permissionIds,
        CancellationToken ct = default)
    {
        if (permissionIds.Count == 0)
            return [];

        var wanted = permissionIds.Select(PermissionId.From).ToHashSet();

        // The catalogue is a few dozen rows and this runs on a write, so the membership test is done
        // in memory rather than as a translated Contains over wrapped identifiers.
        var existing = await context.Permissions.AsNoTracking().Select(x => x.Id).ToListAsync(ct);

        // Unwrapped on the way out: the handler puts the number straight into the error arguments,
        // and a wrapped key would only reach the message through its .Value anyway.
        return wanted.Except(existing).Select(x => x.Value).ToList();
    }

    #endregion

    #region Writes

    public async Task AddAsync(RoleEntity role, CancellationToken ct = default)
        => await context.Roles.AddAsync(role, ct);

    public void SoftDelete(RoleEntity role) => role.MarkDeleted();

    public async Task ReplacePermissionsAsync(
        long roleId,
        IReadOnlyCollection<long> permissionIds,
        CancellationToken ct = default)
    {
        var role = RoleId.From(roleId);
        var wanted = permissionIds.Select(PermissionId.From).ToHashSet();

        var current = await context.RolePermissions
            .Where(x => x.RoleId == role)
            .ToListAsync(ct);

        context.RolePermissions.RemoveRange(current.Where(x => !wanted.Contains(x.PermissionId)));

        // Only the grants the role does not hold yet are inserted; re-saving an unchanged set has to
        // leave the rows alone, or every save would rewrite the whole permission history.
        var alreadyGranted = current.Select(x => x.PermissionId).ToHashSet();
        foreach (var permissionId in wanted.Where(x => !alreadyGranted.Contains(x)))
        {
            context.RolePermissions.Add(RolePermission.Create(role, permissionId));
        }
    }

    public async Task<int> DetachUsersAsync(long roleId, CancellationToken ct = default)
    {
        var role = RoleId.From(roleId);

        var current = await context.UserRoles
            .Where(x => x.RoleId == role)
            .ToListAsync(ct);

        context.UserRoles.RemoveRange(current);

        return current.Count;
    }

    #endregion
}
