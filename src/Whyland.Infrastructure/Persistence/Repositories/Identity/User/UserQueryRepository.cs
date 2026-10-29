using System.Globalization;
using Dapper;
using Whyland.Domain.Entities.Identity.Enums;
using Whyland.Domain.IRepositories.Identity.User;
using Whyland.Infrastructure.Persistence.Extensions;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Dtos.Identity.User;
using UserEntity = Whyland.Domain.Entities.Identity.User;
using ExternalLoginEntity = Whyland.Domain.Entities.Identity.ExternalLogin;

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

    /// <summary>
    /// The users catalog: one account row joined to its profile, with every scalar account
    /// column whitelisted for filtering and sorting. <c>IsDeleted</c> is excluded by the base
    /// predicate, so the only visible rows are live accounts.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {identity}."Users" u
            LEFT JOIN {identity}."UserProfiles" p ON p."UserId" = u."Id"
            """,
        baseWhere: """u."IsDeleted" = FALSE""",
        defaultOrder: """u."CreatedAt" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            // The hash stays out of the whitelist: a listed column is reachable by name from a
            // request, so it must never contain secret material.
            SqlDataTableSpec.ColumnsOf<UserEntity>("u", ["PasswordHash"]),
            [("Avatar", "p.\"Avatar\"", typeof(string))]));

    public Task<UserEntity?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users")
            .Where(x => x.PhoneNumber, phoneNumber)
            .Where(x => x.IsActive, true)
            .Limit(1)
            .GetFirstOrDefaultAsync<UserEntity>(dataSource, ct);

    public Task<UserEntity?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserEntity>.Create($"{identity}.Users")
            .Where(x => x.Id, userId)
            .Limit(1)
            .GetFirstOrDefaultAsync<UserEntity>(dataSource, ct);

    // The assignment table has no soft-delete column of its own, so the guard is written on the
    // role and the permission instead - which is what the statement used to say.
    public Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserRole>.Create($"{identity}.UserRoles", x => x.UserId)
            .IgnoreQueryFilters()
            .Where(x => x.UserId, userId)
            .Join<RolePermission>($"{identity}.RolePermissions", ur => ur.RoleId, rp => rp.RoleId)
            .Join<Role>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .Join<RolePermission, Permission>($"{identity}.Permissions", rp => rp.PermissionId, p => p.Id)
            .WhereFrom<Role>(r => r.IsDeleted, false)
            .WhereFrom<Permission>(p => p.IsDeleted, false)
            .Distinct()
            .ClearSelect()
            .SelectFrom<Permission>(p => p.Key)
            .GetAllAsync<string>(dataSource, ct);

    // The dropdown only ever needs the three identifying columns, so they are listed instead of
    // letting the builder emit every scalar column of the entity.
    public Task<IReadOnlyList<RoleOptionDto>> GetAssignableRolesAsync(CancellationToken ct = default)
        => DapperFluentQueryBuilder<Role>.Create(
                $"{identity}.Roles", x => x.Id, x => x.Name, x => x.Title)
            .Sortable(x => x.Title)
            .OrderBy(nameof(Role.Title))
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
            .Join<UserRole, Role>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .Join<Role, RolePermission>($"{identity}.RolePermissions", r => r.Id, rp => rp.RoleId)
            .Join<RolePermission, Permission>($"{identity}.Permissions", rp => rp.PermissionId, p => p.Id)
            .WhereFrom<Permission>(p => p.Key, permissionKey)
            .WhereFrom<Role>(r => r.IsDeleted, false)
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

    public Task<UserDetailsDto?> GetDetailsAsync(Guid userId, CancellationToken ct = default)
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
            .Where(x => x.Id, userId)
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
            .IgnoreQueryFilters()
            .Where(x => x.UserId, userId)
            .Join<Role>($"{identity}.Roles", ur => ur.RoleId, r => r.Id)
            .WhereFrom<Role>(r => r.IsDeleted, false)
            .ClearSelect()
            .SelectFrom<Role>(r => r.Id)
            .SelectFrom<Role>(r => r.Title)
            .SortableFrom<Role>(r => r.Title)
            .OrderBy(nameof(Role.Title));

        var providers = DapperFluentQueryBuilder<ExternalLoginEntity>.Create($"{identity}.ExternalLogins", x => x.Provider)
            .IgnoreQueryFilters()
            .Where(x => x.UserId, userId)
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

    // One-to-one with the account, so one row is all this can read; the null check keeps an
    // account without an uploaded avatar reading as no avatar.
    public Task<string?> GetAvatarAsync(Guid userId, CancellationToken ct = default)
        => DapperFluentQueryBuilder<UserProfile>.Create($"{identity}.UserProfiles", x => x.Avatar)
            .Where(x => x.UserId, userId)
            .Where(x => x.Avatar, SqlComparisonType.NotEqual, null)
            .Limit(1)
            .GetFirstOrDefaultAsync<string>(dataSource, ct);

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
