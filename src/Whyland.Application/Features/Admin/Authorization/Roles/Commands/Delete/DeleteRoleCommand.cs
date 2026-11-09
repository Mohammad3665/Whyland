namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Delete;

/// <summary>
/// Soft-deletes an existing custom role by its identifier.
/// <para>
/// Deleting a role also takes it away from the accounts that hold it and revokes everything it
/// granted: the join rows carry no delete flag, so a role hidden only by its own <c>IsDeleted</c>
/// would keep handing its permissions out through the effective-permission read.
/// </para>
/// </summary>
/// <param name="Id">The identifier of the role to soft-delete.</param>
public sealed record DeleteRoleCommand(RoleId Id) : ICommand;
