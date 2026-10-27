namespace Whyland.Domain.IRepositories.Identity.ExternalLogin;

/// <summary>
/// Command-side repository for the links between a local user and an external identity
/// provider such as Google (s5.4). Persistence goes through IUnitOfWork.SaveAsync().
/// </summary>
public interface IExternalLoginCommandRepository
{
    /// <summary>Stages a new provider link; persisted on SaveAsync.</summary>
    Task AddAsync(Entities.Identity.ExternalLogin login, CancellationToken ct = default);

    /// <summary>Returns the existing link for a provider subject, or null when the provider identity is unknown.</summary>
    Task<Entities.Identity.ExternalLogin?> GetByProviderKeyAsync(
        string provider,
        string providerKey,
        CancellationToken ct = default);
}
