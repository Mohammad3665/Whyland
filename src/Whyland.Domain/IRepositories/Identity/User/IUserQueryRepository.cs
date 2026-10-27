using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Dtos.Identity.User;
using Whyland.Shared.Filters;

namespace Whyland.Domain.IRepositories.Identity.User;

/// <summary>
/// Query-side repository for the User aggregate: single-row auth reads plus the admin
/// catalog and details reads. Like every query repository, it is served by Dapper and
/// never by the EF Core context; command repositories are the EF Core side.
/// </summary>
public interface IUserQueryRepository
{
    /// <summary>Returns the active, non-deleted user with the given phone, or null.</summary>
    Task<Entities.Identity.User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Returns the stored account row for an id, or null when there is none. The change-password
    /// page needs it to decide what to ask for: a number that is missing must be verified first,
    /// and a stored password must be proven before it is replaced.
    /// </summary>
    Task<Entities.Identity.User?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the effective permission keys of a user (union of all its roles'
    /// permissions). These keys become the "permissions" claims (s6.4).
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the avatar path/URL of the user's profile, or null when no
    /// profile exists or no avatar has been set.
    /// </summary>
    Task<string?> GetAvatarAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Reads the admin users table through the generic filter pipeline: every condition
    /// (field/operator/value) and the sort column are resolved against the account properties,
    /// so a new column never needs its own query.
    /// </summary>
    /// <param name="filter">Paging, sorting and the search field/value.</param>
    /// <param name="conditions">Extra conditions ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<DataTableResult<UserListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);

    /// <summary>
    /// Reads everything the admin details modal shows for one account: identity fields,
    /// password state, roles, profile data, provider links and the login-security counters.
    /// </summary>
    /// <param name="userId">The account to read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<UserDetailsDto?> GetDetailsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns every role that can be assigned to a user, ordered by title. Non-system roles
    /// included so the admin can create and assign its own ones.
    /// </summary>
    Task<IReadOnlyList<RoleOptionDto>> GetAssignableRolesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the accounts that effectively hold <paramref name="permissionKey"/> through their
    /// roles, as id/name pairs, so a control that refers to a user — the «created by» / «last
    /// edited by» filters of the catalog — lists the people who could actually have done it and
    /// never asks the admin to paste a guid.
    /// </summary>
    /// <param name="permissionKey">The permission key the account must own, e.g. "user.create".</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(
        string permissionKey,
        CancellationToken ct = default);
}