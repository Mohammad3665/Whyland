using System.Globalization;
using Dapper;
using Whyland.Domain.Entities.Identity.Enums;
using Whyland.Domain.IRepositories.Identity.User;
using Whyland.Infrastructure.Persistence.Extensions;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Dtos.Identity.User;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// Query repository for the User aggregate. Every read is Dapper over
/// <paramref name="dataSource"/>: the single-row auth reads use plain SQL and the admin
/// catalog uses the generic data-table builder, so no field has its own query here and the
/// EF Core context never appears in this class.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class UserQueryRepository(NpgsqlDataSource dataSource) : IUserQueryRepository
{
    /// <summary>
    /// The users catalog: one account row joined to its profile, with every scalar account
    /// column whitelisted for filtering and sorting. <c>IsDeleted</c> is excluded by the base
    /// predicate, so the only visible rows are live accounts.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: """
            identity."Users" u
            LEFT JOIN identity."UserProfiles" p ON p."UserId" = u."Id"
            """,
        baseWhere: """u."IsDeleted" = FALSE""",
        defaultOrder: """u."CreatedAt" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            // The hash stays out of the whitelist: a listed column is reachable by name from a
            // request, so it must never contain secret material.
            SqlDataTableSpec.ColumnsOf<Domain.Entities.Identity.User>("u", ["PasswordHash"]),
            [("Avatar", "p.\"Avatar\"", typeof(string))]));

    public async Task<Domain.Entities.Identity.User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
    {
        const string sql = """
            SELECT *
            FROM identity."Users"
            WHERE "PhoneNumber" = @PhoneNumber
              AND "IsActive" = true
            LIMIT 1
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<Domain.Entities.Identity.User>(
            new CommandDefinition(sql, new { PhoneNumber = phoneNumber }, cancellationToken: ct));
    }

    public async Task<Domain.Entities.Identity.User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT *
            FROM identity."Users"
            WHERE "Id" = @UserId
              AND "IsDeleted" = FALSE
            LIMIT 1
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<Domain.Entities.Identity.User>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT p."Key"
            FROM identity."UserRoles" ur
            INNER JOIN identity."RolePermissions" rp ON rp."RoleId" = ur."RoleId"
            INNER JOIN identity."Roles" r ON r."Id" = ur."RoleId"
            INNER JOIN identity."Permissions" p ON p."Id" = rp."PermissionId"
            WHERE ur."UserId" = @UserId
              AND r."IsDeleted" = FALSE
              AND p."IsDeleted" = FALSE
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var keys = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        return keys.AsList();
    }

    public async Task<IReadOnlyList<RoleOptionDto>> GetAssignableRolesAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT "Id", "Name", "Title"
            FROM identity."Roles"
            WHERE "IsDeleted" = FALSE
            ORDER BY "Title" ASC
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var roles = await connection.QueryAsync<RoleOptionDto>(
            new CommandDefinition(sql, cancellationToken: ct));

        return roles.AsList();
    }

    public async Task<IReadOnlyList<UserOptionDto>> GetUserOptionsAsync(
        string permissionKey,
        CancellationToken ct = default)
    {
        // Only the accounts that really can do the action behind the picker: the key resolves
        // through the same role graph that issues the "permissions" claims at login. Roles and
        // permissions are soft-deletable, so a revoked role or a retired key must not resurface
        // here (Dapper applies no query filter of its own).
        const string sql = """
            SELECT DISTINCT u."Id", u."FirstName" || ' ' || u."LastName" AS "Name"
            FROM identity."Users" u
            INNER JOIN identity."UserRoles" ur ON ur."UserId" = u."Id"
            INNER JOIN identity."Roles" r ON r."Id" = ur."RoleId"
            INNER JOIN identity."RolePermissions" rp ON rp."RoleId" = ur."RoleId"
            INNER JOIN identity."Permissions" p ON p."Id" = rp."PermissionId"
            WHERE p."Key" = @PermissionKey
              AND u."IsDeleted" = FALSE
              AND u."IsActive" = TRUE
              AND r."IsDeleted" = FALSE
              AND p."IsDeleted" = FALSE
            ORDER BY "Name" ASC
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var options = await connection.QueryAsync<UserOptionDto>(
            new CommandDefinition(sql, new { PermissionKey = permissionKey }, cancellationToken: ct));

        return options.AsList();
    }

    public Task<DataTableResult<UserListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
    {
        // Paging, sorting, the searched field and every chip are resolved by the generic
        // builder against TableSpec, so a new column is a whitelist entry, never a new query.
        return dataSource.ToDataTableAsync<UserListItemDto>(TableSpec, filter, conditions, ct);
    }

    public async Task<UserDetailsDto?> GetDetailsAsync(Guid userId, CancellationToken ct = default)
    {
        // One round-trip, three readers: the account row (with its joined profile), the
        // assigned roles and the linked external providers. A missing profile row is a left
        // join on purpose, so the labels fall back to their defaults instead of hiding the user.
        const string sql = """
            SELECT u."Id", u."FirstName", u."LastName", u."Email", u."PhoneNumber", u."IsActive",
                   (u."PasswordHash" IS NOT NULL) AS "HasPassword",
                   u."LastLoginAt", u."FailedLoginAttempts", u."LockoutUntilUtc",
                   u."CreatedAt", u."UpdatedAt",
                   p."BirthDate", p."Gender", p."Avatar"
            FROM identity."Users" u
            LEFT JOIN identity."UserProfiles" p ON p."UserId" = u."Id"
            WHERE u."Id" = @UserId
              AND u."IsDeleted" = FALSE
            LIMIT 1;

            SELECT r."Id", r."Title"
            FROM identity."UserRoles" ur
            INNER JOIN identity."Roles" r ON r."Id" = ur."RoleId"
            WHERE ur."UserId" = @UserId
              AND r."IsDeleted" = FALSE
            ORDER BY r."Title" ASC;

            SELECT DISTINCT "Provider"
            FROM identity."ExternalLogins"
            WHERE "UserId" = @UserId;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        var account = await multi.ReadFirstOrDefaultAsync<UserDetailsRow>();
        if (account is null)
            return null;

        var roles = (await multi.ReadAsync<UserRoleRow>()).AsList();
        var providers = (await multi.ReadAsync<string>()).AsList();

        return new UserDetailsDto(
            account.Id,
            account.FirstName,
            account.LastName,
            account.Email,
            account.PhoneNumber,
            account.IsActive,
            account.HasPassword,
            roles.Select(x => x.Title).ToList(),
            roles.Select(x => x.Id).ToList(),
            account.Avatar,
            account.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            account.Gender is null ? null : Enum.GetName(typeof(Gender), account.Gender.Value),
            providers,
            account.LastLoginAt,
            account.FailedLoginAttempts,
            account.LockoutUntilUtc,
            account.CreatedAt,
            account.UpdatedAt);
    }

    public async Task<string?> GetAvatarAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p."Avatar"
            FROM identity."UserProfiles" p
            WHERE p."UserId" = @UserId
              AND p."Avatar" IS NOT NULL
            LIMIT 1
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }

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
