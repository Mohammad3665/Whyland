namespace Whyland.Shared.Dtos.Authorization.Role;

/// <summary>
/// One row of the admin roles table, and at the same time everything the details modal shows: the
/// role's own fields, the two counts and the audit columns, so opening the modal is a lookup in the
/// rows the table already holds and never a second round-trip. The two counts replace the fake
/// columns the demo markup carried; they are correlated subqueries rather than a join, so a role
/// keeps exactly one row no matter how many users or grants it has and the table needs no distinct.
/// </summary>
/// <param name="Id">The role identifier.</param>
/// <param name="Name">The unique role name the code and the seeders refer to.</param>
/// <param name="Title">The display title shown to operators.</param>
/// <param name="IsSystem">Whether the role is protected: the page renders it read-only.</param>
/// <param name="UserCount">How many accounts currently hold this role.</param>
/// <param name="PermissionCount">How many permissions are granted to this role.</param>
/// <param name="CreatedAt">UTC time the role was created.</param>
/// <param name="CreatedBy">Identifier of the account that created the row, when the seeder did not.</param>
/// <param name="CreatedByName">
/// The full name of that account, read by joining it to its key. The modal shows a name rather than
/// a guid an operator cannot read; null when the role has no author, which the page renders as a dash.
/// </param>
/// <param name="UpdatedAt">UTC time of the last modification; null until the first update.</param>
/// <param name="UpdatedBy">Identifier of the account that last changed the row, when any.</param>
/// <param name="UpdatedByName">The full name of the account behind <paramref name="UpdatedBy"/>.</param>
public sealed record RoleListItemDto(
    long Id,
    string Name,
    string Title,
    bool IsSystem,
    long UserCount,
    long PermissionCount,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName);
