using ExternalLoginEntity = Whyland.Domain.Model.Identity.ExternalLogin.Entities.ExternalLogin;

namespace Whyland.Domain.Model.Identity.ExternalLogin.Repositories;

/// <summary>
/// Command-side repository for the links between a local user and an external identity
/// provider such as Google (s5.4). Persistence goes through IUnitOfWork.SaveAsync().
/// </summary>
public interface IExternalLoginCommandRepository
{
    /// <summary>Stages a new provider link; persisted on SaveAsync.</summary>
    /// <param name="login">The external login link entity to add.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task AddAsync(ExternalLoginEntity login, CancellationToken ct = default);

    /// <summary>Returns the existing link for a provider subject, or null when the provider identity is unknown.</summary>
    /// <param name="provider">The external provider name, such as <c>Google</c>.</param>
    /// <param name="providerKey">The unique subject identifier issued by the provider.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<ExternalLoginEntity?> GetByProviderKeyAsync(
        string provider,
        string providerKey,
        CancellationToken ct = default);
}