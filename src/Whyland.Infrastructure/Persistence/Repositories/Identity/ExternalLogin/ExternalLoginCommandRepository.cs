using Whyland.Domain.Model.Identity.ExternalLogin.Repositories;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.ExternalLogin;

/// <summary>
/// EF Core command repository for external provider links (s5.4), on the shared scoped
/// WhylandDbContext. The (Provider, ProviderKey) pair is unique, so at most one row is returned.
/// </summary>
public sealed class ExternalLoginCommandRepository(WhylandDbContext context) : IExternalLoginCommandRepository
{
    public async Task AddAsync(Domain.Model.Identity.ExternalLogin.Entities.ExternalLogin login, CancellationToken ct = default)
        => await context.ExternalLogins.AddAsync(login, ct);

    public Task<Domain.Model.Identity.ExternalLogin.Entities.ExternalLogin?> GetByProviderKeyAsync(
        string provider,
        string providerKey,
        CancellationToken ct = default)
        => context.ExternalLogins
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, ct);
}
