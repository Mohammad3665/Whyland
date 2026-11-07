using Whyland.Domain.Model.Identity.InstructorProfile.Repositories;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Identity.InstructorProfile;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Dapper read side of instructor profiles. The table runs on the shared data-table engine;
/// details and the eligible-account list are small direct reads.
/// </summary>
public sealed class InstructorProfileQueryRepository(NpgsqlDataSource dataSource)
    : IInstructorProfileQueryRepository
{
    private const string identity = SchemaConstants.Identity;
    private const string catalog = SchemaConstants.Catalog;

    private const string ProfileImageUrlColumn = """
        CASE WHEN ip."ProfileImage" IS NULL OR ip."ProfileImage" = ''
             THEN NULL
             ELSE '/uploads/images/' || ip."ProfileImage" END
        """;

    private const string CourseCountColumn = """
        COALESCE((
            SELECT COUNT(*)::int
            FROM catalog."CourseInstructors" ci
            JOIN catalog."Courses" c ON c."Id" = ci."CourseId" AND c."IsDeleted" = FALSE
            WHERE ci."InstructorProfileId" = ip."Id"
        ), 0)
        """;

    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {identity}."InstructorProfiles" ip
            JOIN {identity}."Users" u ON u."Id" = ip."UserId"
            """,
        baseWhere: "TRUE",
        defaultOrder: """ip."CreatedAt" DESC, ip."Id" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile>(
                "ip", ["ProfileImage"]),
            [
                ("FirstName", "u.\"FirstName\"", typeof(string)),
                ("LastName", "u.\"LastName\"", typeof(string)),
                ("Email", "u.\"Email\"", typeof(string)),
                ("PhoneNumber", "u.\"PhoneNumber\"", typeof(string)),
                ("ProfileImageUrl", ProfileImageUrlColumn, typeof(string)),
                ("CourseCount", CourseCountColumn, typeof(int)),
            ]),
        searchColumns:
        [
            "FirstName",
            "LastName",
            "Email",
            "PhoneNumber",
            "Specialization",
        ]);

    public Task<DataTableResult<InstructorProfileListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<InstructorProfileListItemDto>(dataSource, filter, conditions, ct);

    public Task<InstructorProfileDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => GetDetailsAsync("ip.\"Id\" = @id", new { id }, ct);

    public Task<InstructorProfileDetailsDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => GetDetailsAsync("ip.\"UserId\" = @userId", new { userId }, ct);

    public async Task<IReadOnlyList<InstructorUserOptionDto>> GetEligibleUsersAsync(
        CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT u."Id",
                   u."FirstName" || ' ' || u."LastName" AS "Name",
                   u."Email",
                   u."PhoneNumber"
            FROM {identity}."Users" u
            WHERE u."IsActive" = TRUE
              AND NOT EXISTS (
                  SELECT 1 FROM {identity}."InstructorProfiles" ip
                  WHERE ip."UserId" = u."Id")
            ORDER BY u."FirstName", u."LastName";
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var users = await connection.QueryAsync<InstructorUserOptionDto>(
            new CommandDefinition(sql, cancellationToken: ct));

        return users.AsList();
    }

    private async Task<InstructorProfileDetailsDto?> GetDetailsAsync(
        string predicate,
        object parameters,
        CancellationToken ct)
    {
        var sql = $"""
            SELECT ip."Id",
                   ip."UserId",
                   u."FirstName",
                   u."LastName",
                   u."Email",
                   u."PhoneNumber",
                   {ProfileImageUrlColumn} AS "ProfileImageUrl",
                   ip."Bio",
                   ip."Biography",
                   ip."Specialization",
                   ip."Website",
                   ip."LinkedIn",
                   ip."Instagram",
                   ip."ShebaNumber",
                   ip."BankAccountNumber",
                   {CourseCountColumn} AS "CourseCount",
                   ip."CreatedAt",
                   ip."UpdatedAt"
            FROM {identity}."InstructorProfiles" ip
            JOIN {identity}."Users" u ON u."Id" = ip."UserId"
            WHERE {predicate};
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<InstructorProfileDetailsDto>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }
}
