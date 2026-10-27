using Whyland.Domain.IRepositories.Identity.ExternalLogin;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.ExternalLogin;

/// <summary>
/// EF Core command repository for external provider links (s5.4), on the shared scoped
/// WhylandDbContext. The (Provider, ProviderKey) pair is unique, so at most one row is returned.
/// </summary>
public sealed class ExternalLoginCommandRepository(WhylandDbContext context) : IExternalLoginCommandRepository
{
    public async Task AddAsync(Domain.Entities.Identity.ExternalLogin login, CancellationToken ct = default)
        => await context.ExternalLogins.AddAsync(login, ct);

    public Task<Domain.Entities.Identity.ExternalLogin?> GetByProviderKeyAsync(
        string provider,
        string providerKey,
        CancellationToken ct = default)
        => context.ExternalLogins
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, ct);
}
