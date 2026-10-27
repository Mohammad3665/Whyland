using Whyland.Shared.Dtos.Authorization.Role;

namespace Whyland.Application.Features.Admin.Users.Queries.GetRoles;

/// <summary>
/// Reads every role that the user modal offers in its multi-select.
/// </summary>
public sealed record GetAssignableRolesQuery : IQuery<IReadOnlyList<RoleOptionDto>>;
