using System.Globalization;
using Whyland.Domain.Model.Identity.UserProfile.Enums;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Dtos.Identity.User;
using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;
using ExternalLoginEntity = Whyland.Domain.Model.Identity.ExternalLogin.Entities.ExternalLogin;
using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// Query repository for the User aggregate. Every read is Dapper over
/// <paramref name="dataSource"/>: the statements are written as builder calls and the admin
/// catalog runs on the generic data-table engine, so no field has its own query here and the
/// EF Core context never appears in this class. <see cref="GetDetailsAsync"/> sends its three
/// statements as one <see cref="DapperSqlBatch"/>, which keeps that read at one round-trip.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class UserQueryRepository(NpgsqlDataSource dataSource) : IUserQueryRepository
{
    const string identity = SchemaConstants.Identity;

    /// <summary>Public prefix every stored image path is served under.</summary>
    private const string UploadsRoot = "/uploads/images/";

    /// <summary>
    /// The accounts that hold one role, written as a test on the row rather than as a column:
    /// the assignment table is what says so, and a retired role is excluded the way every other
    /// read of this graph does. The engine fills <c>{value}</c> with a parameter.
    /// </summary>
    private static readonly string RoleMembership = $$"""
        EXISTS (SELECT 1 FROM {{identity}}."UserRoles" ur
                JOIN {{identity}}."Roles" ro ON ro."Id" = ur."RoleId" AND ro."IsDeleted" = FALSE
                WHERE ur."UserId" = u."Id" AND ur."RoleId" = {value})
        """;

    /// <summary>
    /// The instructor image is stored as a relative path, while the table renders an image URL;
    /// the expression owns that conversion so the browser receives a usable source. An instructor
    /// image wins over the ordinary profile avatar, because it is the picture the account chose
    /// for the public instructor profile.
    /// </summary>
    private static readonly string AvatarColumn = $$"""
        COALESCE(
            CASE WHEN NULLIF(TRIM(ip."ProfileImage"), '') IS NULL THEN NULL
                 ELSE '{{UploadsRoot}}' || TRIM(ip."ProfileImage") END,
            p."Avatar")
        """;

    /// <summary>
    /// The user's password hash stays out of the whitelist: a listed column is reachable by name
    /// from a request, so it must never contain secret material.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {identity}."Users" u
            LEFT JOIN {identity}."UserProfiles" p ON p."UserId" = u."Id"
            LEFT JOIN {identity}."InstructorProfiles" ip ON ip."UserId" = u."Id"
            """,
        baseWhere: "TRUE",
        defaultOrder: """u."CreatedAt" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<UserEntity>("u", ["PasswordHash"]),
            [("Avatar", AvatarColumn, typeof(string))],
            // Declared by the id the assignment table stores, so the picked role is converted
            // before it is bound and a non-numeric value costs the condition, not a query error.
            [("Role", RoleMembership, typeof(long))]));

    public Task<UserEntity?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users")
            .Where(x => x.PhoneNumber, phoneNumber)
            .Where(x => x.IsActive, true)
            .Limit(1)
            .GetFirstOrDefaultAsync<UserEntity>(dataSource, ct);

    public Task<UserEntity?> GetByIdAsync(UserId userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users")
            .Where(x => x.Id, userId.Value)
            .Limit(1)
            .GetFirstOrDefaultAsync<UserEntity>(dataSource, ct);

    // The assignment table has no soft-delete column of its own - the builder only guards a table
    // whose entity really is soft-deletable - so the live-row filter is written on the role and the
    // permission instead, which is what the statement used to say.
    public Task<IReadOnlyList<string>> GetPermissionKeysAsync(UserId userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserRole>.Create($"{identity}.UserRoles", x => x.UserId)
            .Where(x => x.UserId, userId.Value)
            .Join<RolePermission>($"{identity}.RolePermissions", ur => ur.RoleId, rp => rp.RoleId)
            .Join<RoleEntity>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .Join<RolePermission, Permission>($"{identity}.Permissions", rp => rp.PermissionId, p => p.Id)
            .WhereFrom<RoleEntity>(r => r.IsDeleted, false)
            .WhereFrom<Permission>(p => p.IsDeleted, false)
            .Distinct()
            .ClearSelect()
            .SelectFrom<Permission>(p => p.Key)
            .GetAllAsync<string>(dataSource, ct);

    // The dropdown only ever needs the three identifying columns, so they are listed instead of
    // letting the builder emit every scalar column of the entity.
    public Task<IReadOnlyList<RoleOptionDto>> GetAssignableRolesAsync(CancellationToken ct = default)
        => DapperFluentQueryBuilder<RoleEntity>.Create(
                $"{identity}.Roles", x => x.Id, x => x.Name, x => x.Title)
            .Sortable(x => x.Title)
            .OrderBy(nameof(RoleEntity.Title))
            .GetAllAsync<RoleOptionDto>(dataSource, ct);

    // Only the accounts that really can do the action behind the picker: the key resolves through
    // the same role graph that issues the "permissions" claims at login, and a revoked role or a
    // retired key stays out because both tables are guarded here (Dapper applies no filter itself).
    public Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(
        string permissionKey,
        CancellationToken ct = default)
    {
        Func<string, string> column = DapperFluentQueryBuilder<UserEntity>.RootColumn;

        return DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users", x => x.Id)
            .Where(x => x.IsActive, true)
            .Join<UserRole>($"{identity}.UserRoles", u => u.Id, ur => ur.UserId)
            .Join<UserRole, RoleEntity>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .Join<RoleEntity, RolePermission>($"{identity}.RolePermissions", r => r.Id, rp => rp.RoleId)
            .Join<RolePermission, Permission>($"{identity}.Permissions", rp => rp.PermissionId, p => p.Id)
            .WhereFrom<Permission>(p => p.Key, permissionKey)
            .WhereFrom<RoleEntity>(r => r.IsDeleted, false)
            .WhereFrom<Permission>(p => p.IsDeleted, false)
            .Distinct()
            .SelectExpression(
                $"{column(nameof(UserEntity.FirstName))} || ' ' || {column(nameof(UserEntity.LastName))}",
                "Name")
            // PostgreSQL reads the output alias here, so the picker is ordered by the joined name
            // and a request may sort or filter by it under that same key.
            .SortableExpression("\"Name\"", "Name", typeof(string))
            .OrderBy("Name")
            .GetAllAsync<UserOptionDto>(dataSource, ct);
    }

    public Task<DataTableResult<UserListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
    {
        // Paging, sorting, the searched field and every chip are resolved by the generic engine
        // against TableSpec, so a new column is a whitelist entry, never a new query.
        return TableSpec.GetDataTableAsync<UserListItemDto>(dataSource, filter, conditions, ct);
    }

    public Task<UserDetailsDto?> GetDetailsAsync(UserId userId, CancellationToken ct = default)
    {
        // One round-trip, three readers: the account row (with its joined profile), the assigned
        // roles and the linked external providers. A missing profile row is a left join on purpose,
        // so the labels fall back to their defaults instead of hiding the user.
        //
        // The row types are positional records, so the columns are listed in the order their
        // constructor declares them: Dapper matches a result set onto that constructor and says so
        // when the shapes disagree.
        var account = DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users", x => x.Id)
            .LeftJoin<UserProfile>($"{identity}.UserProfiles", u => u.Id, p => p.UserId)
            .Where(x => x.Id, userId.Value)
            .ClearSelect()
            .Select(x => x.Id)
            .Select(x => x.FirstName)
            .Select(x => x.LastName)
            .Select(x => x.Email)
            .Select(x => x.PhoneNumber)
            .Select(x => x.IsActive)
            // The hash is never a column of this read; only whether one exists is shown.
            .SelectExpression($"""({DapperFluentQueryBuilder<UserEntity>.RootColumn(nameof(UserEntity.PasswordHash))} IS NOT NULL)""", nameof(UserDetailsRow.HasPassword))
            .Select(x => x.LastLoginAt)
            .Select(x => x.FailedLoginAttempts)
            .Select(x => x.LockoutUntilUtc)
            .Select(x => x.CreatedAt)
            .Select(x => x.UpdatedAt)
            .SelectFrom<UserProfile>(p => p.BirthDate)
            .SelectFrom<UserProfile>(p => p.Gender)
            .SelectFrom<UserProfile>(p => p.Avatar)
            .Limit(1);

        var roles = DapperFluentQueryBuilder<UserRole>.Create($"{identity}.UserRoles", x => x.UserId)
            .Where(x => x.UserId, userId.Value)
            .Join<RoleEntity>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .WhereFrom<RoleEntity>(r => r.IsDeleted, false)
            .ClearSelect()
            .SelectFrom<RoleEntity>(r => r.Id)
            .SelectFrom<RoleEntity>(r => r.Title)
            .SortableFrom<RoleEntity>(r => r.Title)
            .OrderBy(nameof(RoleEntity.Title));

        var providers = DapperFluentQueryBuilder<ExternalLoginEntity>.Create($"{identity}.ExternalLogins", x => x.Provider)
            .Where(x => x.UserId, userId.Value)
            .Distinct();

        return DapperSqlBatch.RunAsync(
            dataSource,
            multi =>
            {
                var row = multi.ReadFirstOrDefault<UserDetailsRow>();
                if (row is null)
                    return null;

                var assigned = multi.Read<UserRoleRow>().AsList();
                var links = multi.Read<string>().AsList();

                return new UserDetailsDto(
                    row.Id,
                    row.FirstName,
                    row.LastName,
                    row.Email,
                    row.PhoneNumber,
                    row.IsActive,
                    row.HasPassword,
                    assigned.Select(x => x.Title).ToList(),
                    assigned.Select(x => x.Id).ToList(),
                    row.Avatar,
                    row.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    row.Gender is null ? null : Enum.GetName(typeof(Gender), row.Gender.Value),
                    links,
                    row.LastLoginAt,
                    row.FailedLoginAttempts,
                    row.LockoutUntilUtc,
                    row.CreatedAt,
                    row.UpdatedAt);
            },
            ct,
            account,
            roles,
            providers);
    }

    // The header menu row: the account columns the menu shows, with the avatar carried by the same
    // read. The profile is one-to-one with the account, so one row is all this can be; the join is
    // left because an account without a profile row is still a signed-in user, and it then reads as
    // no avatar.
    // The columns are listed in the order UserMenuDto declares them - Dapper binds a result set onto
    // that constructor and reports the mismatch when the shapes disagree.
    public Task<UserMenuDto?> GetMenuAsync(UserId userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users", x => x.Id)
            .LeftJoin<UserProfile>($"{identity}.UserProfiles", u => u.Id, p => p.UserId)
            .Where(x => x.Id, userId.Value)
            .ClearSelect()
            .Select(x => x.FirstName)
            .Select(x => x.LastName)
            .Select(x => x.PhoneNumber)
            .SelectFrom<UserProfile>(p => p.Avatar)
            .Limit(1)
            .GetFirstOrDefaultAsync<UserMenuDto>(dataSource, ct);

    /// <summary>The account row of the details read; profile columns are null without a profile.</summary>
    private sealed record UserDetailsRow(
        Guid Id,
        string FirstName,
        string LastName,
        string? Email,
        string? PhoneNumber,
        bool IsActive,
        bool HasPassword,
        DateTime? LastLoginAt,
        int FailedLoginAttempts,
        DateTime? LockoutUntilUtc,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateOnly? BirthDate,
        int? Gender,
        string? Avatar);

    /// <summary>A role assigned to the account, with its id so the edit form can preselect it.</summary>
    private sealed record UserRoleRow(long Id, string Title);
}
