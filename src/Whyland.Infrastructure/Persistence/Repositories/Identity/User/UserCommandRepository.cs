namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// EF Core command repository for User writes (login flow and the admin users page), on the
/// shared scoped WhylandDbContext; persistence happens through the Unit of Work.
/// </summary>
public sealed class UserCommandRepository(WhylandDbContext context) : IUserCommandRepository
{
    public async Task AddAsync(Domain.Model.Identity.User.Entities.User user, CancellationToken ct = default)
        => await context.Users.AddAsync(user, ct);

    public async Task ReplaceRolesAsync(
        Guid userId, IReadOnlyCollection<long> roleIds, CancellationToken ct = default)
    {
        // The panel sends raw numbers, so both keys are wrapped before the diff below.
        var actor = UserId.From(userId);
        var wanted = roleIds.Select(RoleId.From).ToHashSet();

        // The ordinary-user role is a floor, not a choice: whatever the modal sent, an account
        // without it cannot do anything on the site, so it is unioned in here rather than trusted to
        // every caller to remember to include it.
        var defaultRole = await DefaultRoleIdAsync(ct);
        if (defaultRole is not null)
            wanted.Add(defaultRole.Value);

        var current = await context.UserRoles
            .Where(x => x.UserId == actor)
            .ToListAsync(ct);

        context.UserRoles.RemoveRange(current.Where(x => !wanted.Contains(x.RoleId)));

        var alreadyAssigned = current.Select(x => x.RoleId).ToHashSet();
        foreach (var roleId in wanted.Where(x => !alreadyAssigned.Contains(x)))
        {
            context.UserRoles.Add(UserRole.Create(actor, roleId));
        }
    }

    // Registration and provider sign-in build an account without a role list, and the seeder is the
    // only other writer of assignments; this is the one place the floor is expressed for them.
    public async Task EnsureDefaultRoleAsync(Guid userId, CancellationToken ct = default)
    {
        var defaultRole = await DefaultRoleIdAsync(ct);
        if (defaultRole is null)
            return;

        var actor = UserId.From(userId);

        if (await context.UserRoles.AnyAsync(x => x.UserId == actor && x.RoleId == defaultRole, ct))
            return;

        context.UserRoles.Add(UserRole.Create(actor, defaultRole.Value));
    }

    /// <summary>
    /// The identifier of the role every account must hold, looked up by the name the seeder inserts
    /// so the two can never disagree on a re-typed string. Null only when that seed row is absent —
    /// a database that was never seeded, where no assignment can be written anyway.
    /// </summary>
    private async Task<RoleId?> DefaultRoleIdAsync(CancellationToken ct)
    {
        var role = await context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == RoleNames.User, ct);

        return role?.Id;
    }

    public void TouchLastLogin(Domain.Model.Identity.User.Entities.User user)
    {
        user.RecordSuccessfulLogin(DateTime.UtcNow);
        context.Users.Update(user);
    }

    // IsActive is deliberately not filtered: the caller must be able to tell a disabled account from
    // an unknown one without leaking either to the client. The soft-delete filter is applied by the
    // DbContext itself.
    public Task<Domain.Model.Identity.User.Entities.User?> GetTrackedByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, ct);

    public Task<Domain.Model.Identity.User.Entities.User?> GetTrackedByEmailAsync(string email, CancellationToken ct = default)
        => context.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<Domain.Model.Identity.User.Entities.User?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = UserId.From(id);
        return context.Users.FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    public Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        var actor = UserId.From(userId);
        return context.UserRoles.AnyAsync(x => x.UserId == actor && x.Role.Name == roleName, ct);
    }
}