using Dapper;
using Whyland.Domain.IRepositories.Identity.User;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.User;

/// <summary>
/// EF Core query repository for the User aggregate (auth reads).
/// </summary>
public sealed class UserQueryRepository(NpgsqlDataSource dataSource) : IUserQueryRepository
{
    public async Task<Domain.Entities.Identity.User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
    {
        const string sql = """
            SELECT *
            FROM identity."Users"
            WHERE "PhoneNumber" = @PhoneNumber
              AND "IsActive" = true
            LIMIT 1
            """;

        using var connection = await dataSource.OpenConnectionAsync(ct);
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

        using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<Domain.Entities.Identity.User>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT p."Key"
            FROM identity."UserRoles" ur
            INNER JOIN identity."RolePermissions" rp ON rp."RoleId" = ur."RoleId"
            INNER JOIN identity."Permissions" p ON p."Id" = rp."PermissionId"
            WHERE ur."UserId" = @UserId
            """;

        using var connection = await dataSource.OpenConnectionAsync(ct);
        var keys = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        return keys.AsList();
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

        using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }
}