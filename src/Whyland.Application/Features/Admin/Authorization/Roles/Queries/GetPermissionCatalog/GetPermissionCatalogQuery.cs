using Whyland.Shared.Dtos.Authorization.Permission;

namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetPermissionCatalog;

/// <summary>
/// Reads the live permission catalogue that the roles editor builds its tree from. The rows carry the
/// key, not only the label, because the tree is grouped by the key's prefix ("users.", "roles.") and
/// the panel shows the Persian name next to each box.
/// </summary>
public sealed record GetPermissionCatalogQuery : IQuery<IReadOnlyList<PermissionOptionDto>>;
