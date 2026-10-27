using Whyland.Domain.IRepositories.Identity.User;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// EF Core command repository for User writes (login flow and the admin users page), on the
/// shared scoped WhylandDbContext; persistence happens through the Unit of Work.
/// </summary>
public sealed class UserCommandRepository(WhylandDbContext context) : IUserCommandRepository
{
    public async Task AddAsync(Domain.Entities.Identity.User user, CancellationToken ct = default)
        => await context.Users.AddAsync(user, ct);

    public async Task ReplaceRolesAsync(
        Guid userId, IReadOnlyCollection<long> roleIds, CancellationToken ct = default)
    {
        var current = await context.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        context.UserRoles.RemoveRange(current.Where(x => !roleIds.Contains(x.RoleId)));

        var alreadyAssigned = current.Select(x => x.RoleId).ToHashSet();
        foreach (var roleId in roleIds.Where(x => !alreadyAssigned.Contains(x)))
        {
            context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }
    }

    public void TouchLastLogin(Domain.Entities.Identity.User user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        context.Users.Update(user);
    }

    // IsActive is deliberately not filtered: the caller must be able to tell a disabled account from
    // an unknown one without leaking either to the client. The soft-delete filter is applied by the
    // DbContext itself.
    public Task<Domain.Entities.Identity.User?> GetTrackedByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, ct);

    public Task<Domain.Entities.Identity.User?> GetTrackedByEmailAsync(string email, CancellationToken ct = default)
        => context.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<Domain.Entities.Identity.User?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
        => context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
        => context.UserRoles.AnyAsync(x => x.UserId == userId && x.Role.Name == roleName, ct);
}