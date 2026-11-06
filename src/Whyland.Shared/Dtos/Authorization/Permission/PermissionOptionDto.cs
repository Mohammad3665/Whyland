namespace Whyland.Shared.Dtos.Authorization.Permission;

/// <summary>
/// One entry of the permission catalogue the role editor renders its tree from. The list is the
/// table as it is stored, so a key retired from the code disappears from the picker without any
/// migration — and a key the code declares but the seeder never inserted simply is not grantable.
/// </summary>
/// <param name="Id">The permission identifier the grant is written with.</param>
/// <param name="Key">The dotted key evaluated by the authorization handler (e.g. "Users.Create").</param>
/// <param name="Name">The display name of the permission.</param>
public sealed record PermissionOptionDto(
    long Id,
    string Key,
    string Name);
