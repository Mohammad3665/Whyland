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