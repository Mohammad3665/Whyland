using Whyland.Domain.Model.Identity.ExternalLogin.Errors;
using Whyland.Domain.Model.Identity.User.Errors;
using Whyland.Shared.Results;

using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;

namespace Whyland.Domain.Model.Identity.ExternalLogin.Entities;

/// <summary>
/// The link between a user account and an external identity provider (s5.3).
/// Kept separate from <see cref="BaseEntity"/>-style entities because it has no
/// soft-delete requirement; deleting the link simply removes the login option.
/// </summary>
public class ExternalLogin
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="ProviderKey"/>; also the database column size.</summary>
    public const int MaxProviderKeyLength = 256;

    /// <summary>Maximum allowed length of <see cref="Provider"/>; also the database column size.</summary>
    public const int MaxProviderLength = 100;

    #endregion

    #region Properties

    /// <summary>
    /// The unique identifier of the login link.
    /// </summary>
    public ExternalLoginId Id { get; private set; }

    /// <summary>
    /// The locally registered user this provider account maps to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Name of the authentication scheme (e.g. "Google").
    /// </summary>
    public string Provider { get; private set; } = string.Empty;

    /// <summary>
    /// The external provider's unique user identifier.
    /// </summary>
    public string ProviderKey { get; private set; } = string.Empty;

    /// <summary>
    /// UTC time the external login was first linked.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The locally registered user this provider account maps to.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — link accounts through <see cref="Create"/>.
    /// </summary>
    private ExternalLogin() { }

    /// <summary>
    /// Links one provider identity to a local account, after trimming and validating the two claims
    /// the provider released. The timestamp is stamped here rather than by the audit interceptor: this
    /// row is not an <c>IAuditable</c>, so nothing else would ever fill <see cref="CreatedAt"/> in.
    /// </summary>
    /// <param name="userId">The local account the provider identity maps to.</param>
    /// <param name="provider">The authentication scheme name, e.g. "Google". Will be trimmed.</param>
    /// <param name="providerKey">The identifier the provider gave to this user. Will be trimmed.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the link,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<ExternalLogin> Create(UserId userId, string provider, string providerKey)
    {
        var login = new ExternalLogin
        {
            Id = ExternalLoginId.From(Guid.NewGuid()),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        var applied = login.Apply(provider, providerKey);
        return applied.IsFailure ? applied.Error : login;
    }

    /// <summary>
    /// Trims, validates and assigns the two provider claims. Uniqueness of the pair belongs to the
    /// store; on failure the instance is left unchanged.
    /// </summary>
    /// <param name="provider">The authentication scheme name, e.g. "Google". Will be trimmed.</param>
    /// <param name="providerKey">The identifier the provider gave to this user. Will be trimmed.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string provider, string providerKey)
    {
        var trimmedProvider = provider?.Trim();
        var trimmedProviderKey = providerKey?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedProvider))
            return ExternalLoginErrors.FieldRequired(nameof(Provider));

        if (trimmedProvider.Length > MaxProviderLength)
            return ExternalLoginErrors.FieldTooLong(nameof(Provider), MaxProviderLength);

        if (string.IsNullOrWhiteSpace(trimmedProviderKey))
            return ExternalLoginErrors.FieldRequired(nameof(ProviderKey));

        if (trimmedProviderKey.Length > MaxProviderKeyLength)
            return ExternalLoginErrors.FieldTooLong(nameof(ProviderKey), MaxProviderKeyLength);

        Provider = trimmedProvider;
        ProviderKey = trimmedProviderKey;

        return Result.Success();
    }

    #endregion
}
