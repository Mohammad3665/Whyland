using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;
using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Identity.InstructorProfile;
using System.Linq.Expressions;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.InstructorProfile;

/// <summary>
/// Dapper read side of instructor profiles. The table runs on the shared data-table engine;
/// details and the eligible-account list are small direct reads.
/// </summary>
public sealed class InstructorProfileQueryRepository(NpgsqlDataSource dataSource)
    : IInstructorProfileQueryRepository
{
    private const string IdentitySchema = SchemaConstants.Identity;

    private const string ProfileImageExpression = """
        CASE WHEN ip."ProfileImage" IS NULL OR ip."ProfileImage" = ''
             THEN NULL
             ELSE '/uploads/images/' || ip."ProfileImage" END
        """;

    private const string CourseCountExpression = """
        COALESCE((
            SELECT COUNT(*)::int
            FROM catalog."CourseInstructors" ci
            JOIN catalog."Courses" c ON c."Id" = ci."CourseId" AND c."IsDeleted" = FALSE
            WHERE ci."InstructorProfileId" = ip."Id"
        ), 0)
        """;

    private const string From = $"""
        {IdentitySchema}."InstructorProfiles" ip
        JOIN {IdentitySchema}."Users" u ON u."Id" = ip."UserId"
        """;

    private static readonly SqlDataTableSpec TableSpec = new(
        from: From,
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
                ("ProfileImageUrl", ProfileImageExpression, typeof(string)),
                ("CourseCount", CourseCountExpression, typeof(int)),
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

    private async Task<InstructorProfileDetailsDto?> GetDetailsAsync(
        Expression<Func<InstructorProfileEntity, object?>> predicate,
        object value,
        CancellationToken ct)
    {
        const string profileImageExpr = """
            CASE WHEN {0} IS NULL OR {0} = '' THEN NULL
                ELSE '/uploads/images/' || {0} END
            """;

        const string courseCountExpr = """
            COALESCE((
                SELECT COUNT(*)::int
                FROM catalog."CourseInstructors" ci
                JOIN catalog."Courses" c ON c."Id" = ci."CourseId" AND c."IsDeleted" = FALSE
                WHERE ci."InstructorProfileId" = {0}
            ), 0)
            """;

        var profileImage = string.Format(profileImageExpr, DapperFluentQueryBuilder<InstructorProfileEntity>.RootColumn("ProfileImage"));
        var courseCount = string.Format(courseCountExpr, DapperFluentQueryBuilder<InstructorProfileEntity>.RootColumn("Id"));

        var builder = DapperFluentQueryBuilder<InstructorProfileEntity>
            .Create("identity.InstructorProfiles")
            .ClearSelect()
            .Join<UserEntity>("identity.Users", p => p.UserId, u => u.Id)
            .SelectFrom<InstructorProfileEntity>(x => x.Id)
            .SelectFrom<InstructorProfileEntity>(x => x.UserId)
            .SelectFrom<UserEntity>(x => x.FirstName, "FirstName")
            .SelectFrom<UserEntity>(x => x.LastName, "LastName")
            .SelectFrom<UserEntity>(x => x.Email, "Email")
            .SelectFrom<UserEntity>(x => x.PhoneNumber, "PhoneNumber")
            .SelectExpression(profileImage, "ProfileImageUrl")
            .SelectFrom<InstructorProfileEntity>(x => x.Bio)
            .SelectFrom<InstructorProfileEntity>(x => x.Biography)
            .SelectFrom<InstructorProfileEntity>(x => x.Specialization)
            .SelectFrom<InstructorProfileEntity>(x => x.Website)
            .SelectFrom<InstructorProfileEntity>(x => x.LinkedIn)
            .SelectFrom<InstructorProfileEntity>(x => x.Instagram)
            .SelectFrom<InstructorProfileEntity>(x => x.ShebaNumber)
            .SelectFrom<InstructorProfileEntity>(x => x.BankAccountNumber)
            .SelectExpression(courseCount, "CourseCount")
            .SelectFrom<InstructorProfileEntity>(x => x.CreatedAt)
            .SelectFrom<InstructorProfileEntity>(x => x.UpdatedAt)
            .Where(predicate, value)
            .Limit(1);

        var query = builder.Build();

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<InstructorProfileDetailsDto>(
            new CommandDefinition(query.Sql, query.Parameters, cancellationToken: ct));
    }

    public Task<InstructorProfileDetailsDto?> GetByIdAsync(InstructorProfileId id, CancellationToken ct = default)
        => GetDetailsAsync(x => x.Id, id.Value, ct);

    public Task<InstructorProfileDetailsDto?> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => GetDetailsAsync(x => x.UserId, userId.Value, ct);

    public async Task<IReadOnlyList<InstructorUserOptionDto>> GetEligibleUsersAsync(
        CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT u."Id",
                   u."FirstName" || ' ' || u."LastName" AS "Name",
                   u."Email",
                   u."PhoneNumber"
            FROM {IdentitySchema}."Users" u
            WHERE u."IsActive" = TRUE
              AND NOT EXISTS (
                  SELECT 1 FROM {IdentitySchema}."InstructorProfiles" ip
                  WHERE ip."UserId" = u."Id")
            ORDER BY u."FirstName", u."LastName";
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var users = await connection.QueryAsync<InstructorUserOptionDto>(
            new CommandDefinition(sql, cancellationToken: ct));

        return users.AsList();
    }
}