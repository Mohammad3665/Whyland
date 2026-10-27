using Whyland.Domain.IRepositories.Identity.User;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// EF Core command repository for auth-related User writes, on the shared scoped
/// WhylandDbContext; persistence happens through the Unit of Work.
/// </summary>
public sealed class UserCommandRepository(WhylandDbContext context) : IUserCommandRepository
{
    public async Task AddAsync(Domain.Entities.Identity.User user, CancellationToken ct = default)
        => await context.Users.AddAsync(user, ct);

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
}