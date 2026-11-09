using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Authorization.Permission;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Filters;

using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.Roles;

/// <summary>
/// The Dapper query repository for the <c>Role</c> aggregate. The catalog runs on the shared
/// data-table engine, so paging, sorting and filter chips are whitelist business rather than
/// per-column SQL, and the delete flags stay out of that whitelist.
/// </summary>
/// <param name="dataSource">The pooled PostgreSQL data source every statement runs on.</param>
public sealed class RoleQueryRepository(NpgsqlDataSource dataSource) : IRoleQueryRepository
{
    const string identity = SchemaConstants.Identity;

    #region Roles

    /// <summary>
    /// The roles catalog: the role row itself, plus the two counts the table shows as correlated
    /// subqueries. Subqueries rather than a join are deliberate — a join over UserRoles would
    /// multiply the row by its assignments, and the engine has no distinct to undo that.
    /// The two audit joins are the safe kind of join: an account is read by its key, so a role
    /// still yields one row, and the modal shows who made it instead of the guid the audit column
    /// stores. Both the joins and the name columns come from SqlDataTableSpec, so no list page
    /// spells an account differently from another.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {identity}."Roles" r
            {SqlDataTableSpec.AuthorJoin("r", SqlDataTableSpec.CreatedByAlias, "CreatedBy")}
            {SqlDataTableSpec.AuthorJoin("r", SqlDataTableSpec.UpdatedByAlias, "UpdatedBy")}
            """,
        baseWhere: """r."IsDeleted" = FALSE""",
        defaultOrder: """r."Title" ASC""",
        columns: SqlDataTableSpec.WithExtras(
            // The deletion columns are hidden from the picker: a listed column is reachable by name
            // from a request, and a live-row filter is this query's own business, not the operator's.
            SqlDataTableSpec.ColumnsOf<RoleEntity>("r", ["IsDeleted", "DeletedAt", "DeletedBy"]),
            [
                ("UserCount",
                    $"""(SELECT COUNT(*) FROM {identity}."UserRoles" ur WHERE ur."RoleId" = r."Id")""",
                    typeof(long)),
                ("PermissionCount",
                    $"""(SELECT COUNT(*) FROM {identity}."RolePermissions" rp WHERE rp."RoleId" = r."Id")""",
                    typeof(long)),
                .. SqlDataTableSpec.AuthorNames(),
            ]));

    public Task<DataTableResult<RoleListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<RoleListItemDto>(dataSource, filter, conditions, ct);

    // The grant table has no soft-delete column of its own, so the live-row guard belongs on the
    // permission this reads back: a retired key must not come pre-selected into the editor.
    public async Task<IReadOnlyList<PermissionId>> GetGrantedPermissionIdsAsync(
        RoleId roleId,
        CancellationToken ct = default)
    {
        var ids = await DapperFluentQueryBuilder<RolePermission>
            .Create($"{identity}.RolePermissions", x => x.PermissionId)
            .Where(x => x.RoleId, roleId.Value)
            .Join<Permission>($"{identity}.Permissions", rp => rp.PermissionId, p => p.Id)
            .WhereFrom<Permission>(p => p.IsDeleted, false)
            .OrderBy(nameof(RolePermission.PermissionId))
            .GetAllAsync<long>(dataSource, ct);

        return ids.Select(PermissionId.From).ToList();
    }

    #endregion

    #region Permissions

    // Only the three columns the tree needs are listed, in the order PermissionOptionDto declares
    // them: Dapper binds the result set onto that constructor and reports a mismatch when it does not.
    public Task<IReadOnlyList<PermissionOptionDto>> GetPermissionsAsync(CancellationToken ct = default)
        => DapperFluentQueryBuilder<Permission>.Create($"{identity}.Permissions", x => x.Id)
            .Where(x => x.IsDeleted, false)
            .ClearSelect()
            .Select(x => x.Id)
            .Select(x => x.Key)
            .Select(x => x.Name)
            .OrderBy(nameof(Permission.Key))
            .GetAllAsync<PermissionOptionDto>(dataSource, ct);

    #endregion
}
