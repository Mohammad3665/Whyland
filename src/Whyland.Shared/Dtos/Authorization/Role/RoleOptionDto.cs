namespace Whyland.Shared.Dtos.Authorization.Role;

/// <summary>
/// A role as the admin user form needs it: an identifier to submit and a title to display.
/// </summary>
/// <param name="Id">The role identifier.</param>
/// <param name="Name">The unique role name (e.g. "SuperAdmin").</param>
/// <param name="Title">The human-readable title shown in the select.</param>
public sealed record RoleOptionDto(long Id, string Name, string Title);
