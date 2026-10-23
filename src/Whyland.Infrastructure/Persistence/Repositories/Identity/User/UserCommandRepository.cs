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
}