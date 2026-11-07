using System.Globalization;
using Whyland.Domain.Model.Catalog.Course.Repositories;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.Course;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Course;

/// <summary>
/// The Dapper read side of the course catalogue. The admin list runs on the shared data-table
/// engine; the form's reference checks and option lists are three small statements that never
/// need the EF context.
/// </summary>
public sealed class CourseQueryRepository(NpgsqlDataSource dataSource) : ICourseQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;
    private const string identity = SchemaConstants.Identity;

    /// <summary>
    /// Category names as one cell. The subquery keeps the course row unique even when the course
    /// has several categories, which a join would not: the table's page and its two counts all read
    /// the same FROM, so a duplicated row would also corrupt the totals.
    /// </summary>
    private const string CategoryNamesColumn = """
        COALESCE((
            SELECT string_agg(DISTINCT cat."Name", '، ' ORDER BY cat."Name")
            FROM catalog."CourseCategories" cc
            JOIN catalog."Categories" cat ON cat."Id" = cc."CategoryId" AND cat."IsDeleted" = FALSE
            WHERE cc."CourseId" = c."Id"
        ), '')
        """;

    /// <summary>Instructor names as one cell, for the same reason as the category names.</summary>
    private const string InstructorNamesColumn = """
        COALESCE((
            SELECT string_agg(DISTINCT iu."FirstName" || ' ' || iu."LastName", '، ')
            FROM catalog."CourseInstructors" ci
            JOIN identity."InstructorProfiles" ip ON ip."Id" = ci."InstructorProfileId"
            JOIN identity."Users" iu ON iu."Id" = ip."UserId"
            WHERE ci."CourseId" = c."Id"
        ), '')
        """;

    /// <summary>The category filter, expressed as membership of the link table.</summary>
    private const string CategoryMembership = """
        EXISTS (
            SELECT 1 FROM catalog."CourseCategories" fcc
            WHERE fcc."CourseId" = c."Id" AND fcc."CategoryId" = {value}
        )
        """;

    /// <summary>The instructor filter, expressed as membership of the link table.</summary>
    private const string InstructorMembership = """
        EXISTS (
            SELECT 1 FROM catalog."CourseInstructors" fci
            WHERE fci."CourseId" = c."Id" AND fci."InstructorProfileId" = {value}
        )
        """;

    /// <summary>
    /// Public image URLs are derived in SQL from the one stored relative path. The alternative —
    /// storing a URL — would make the database own the web root, and a thumbnail directory move
    /// would need a data migration.
    /// </summary>
    private const string MainImageUrlColumn = """
        CASE WHEN c."MainImage" IS NULL OR c."MainImage" = ''
             THEN NULL
             ELSE '/uploads/images/' || c."MainImage" END
        """;

    private const string ThumbnailUrlColumn = """
        CASE WHEN c."MainImage" IS NULL OR c."MainImage" = ''
             THEN NULL
             ELSE replace('/uploads/images/' || c."MainImage", '/originals/', '/thumbnails/') END
        """;

    /// <summary>
    /// Enums are stored as their numeric value by EF's default convention, while the row DTOs carry
    /// the name so the table can dress them without knowing the numbers. The CASE also keeps the
    /// generic picker working: it sends the name and compares it to the same text expression.
    /// </summary>
    private const string DiscountTypeColumn = """
        CASE c."DiscountType"
            WHEN 1 THEN 'Percentage'
            WHEN 2 THEN 'FixedAmount'
            ELSE 'None' END
        """;

    private const string DisplayStatusColumn = """
        CASE c."DisplayStatus"
            WHEN 1 THEN 'Published'
            WHEN 2 THEN 'Hidden'
            ELSE 'Draft' END
        """;

    /// <summary>
    /// The admin list. `MainImage` is excluded because the row carries its two public URLs instead;
    /// `IsFree`/`IsPaid` are domain-derived properties with no column behind them. Category and
    /// instructor are memberships: they filter rows but are never projected, because one course can
    /// have several of each.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {catalog}."Courses" c
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.CreatedByAlias, "CreatedBy")}
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.UpdatedByAlias, "UpdatedBy")}
            """,
        baseWhere: """c."IsDeleted" = FALSE""",
        defaultOrder: """c."CreatedAt" DESC, c."Id" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<Domain.Model.Catalog.Course.Entities.Course>(
                "c", ["MainImage", "IsFree", "IsPaid"]),
            [
                ("MainImageUrl", MainImageUrlColumn, typeof(string)),
                ("ThumbnailUrl", ThumbnailUrlColumn, typeof(string)),
                ("DiscountType", DiscountTypeColumn, typeof(string)),
                ("DisplayStatus", DisplayStatusColumn, typeof(string)),
                ("CategoryNames", CategoryNamesColumn, typeof(string)),
                ("InstructorNames", InstructorNamesColumn, typeof(string)),
                .. SqlDataTableSpec.AuthorNames(),
            ],
            [
                ("CategoryId", CategoryMembership, typeof(long)),
                ("InstructorProfileId", InstructorMembership, typeof(Guid)),
            ]),
        searchColumns:
        [
            nameof(Domain.Model.Catalog.Course.Entities.Course.Name),
            nameof(Domain.Model.Catalog.Course.Entities.Course.LatinName),
            nameof(Domain.Model.Catalog.Course.Entities.Course.Slug),
        ]);

    /// <summary>Runs one page of the admin listing with its filters and total counts.</summary>
    public Task<DataTableResult<CourseListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<CourseListItemDto>(dataSource, filter, conditions, ct);

    /// <summary>
    /// Reads the editable course, its selected category ids and its selected instructor ids in one
    /// batch. A missing row returns null before the two link lists are read.
    /// </summary>
    public async Task<CourseDetailsDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT c."Id", c."Name", c."LatinName", c."ShortDescription", c."Description",
                   c."Slug", c."MainImage", c."Price", c."DiscountType", c."DiscountValue",
                   c."IsSaleActive", c."DisplayStatus", c."IsFeatured", c."IsAmazing",
                   c."CreatedAt", c."CreatedBy", c."UpdatedAt", c."UpdatedBy",
                   cu."FirstName" || ' ' || cu."LastName" AS "CreatedByName",
                   uu."FirstName" || ' ' || uu."LastName" AS "UpdatedByName"
            FROM {catalog}."Courses" c
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.CreatedByAlias, "CreatedBy")}
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.UpdatedByAlias, "UpdatedBy")}
            WHERE c."Id" = @id AND c."IsDeleted" = FALSE;

            SELECT cc."CategoryId"
            FROM {catalog}."CourseCategories" cc
            WHERE cc."CourseId" = @id
            ORDER BY cc."CategoryId";

            SELECT ci."InstructorProfileId"
            FROM {catalog}."CourseInstructors" ci
            WHERE ci."CourseId" = @id
            ORDER BY ci."InstructorProfileId";
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { id }, cancellationToken: ct));

        var row = await multi.ReadFirstOrDefaultAsync<CourseDetailsRow>();
        if (row is null)
            return null;

        var categoryIds = (await multi.ReadAsync<long>()).ToArray();
        var instructorIds = (await multi.ReadAsync<Guid>()).ToArray();

        return new CourseDetailsDto(
            row.Id,
            row.Name,
            row.LatinName,
            row.ShortDescription,
            row.Description,
            row.Slug,
            BuildPublicUrl(row.MainImage),
            BuildThumbnailUrl(row.MainImage),
            row.Price,
            EnumName<DiscountType>(row.DiscountType, nameof(DiscountType.None)),
            row.DiscountValue,
            row.IsSaleActive,
            EnumName<DisplayStatus>(row.DisplayStatus, nameof(DisplayStatus.Draft)),
            row.IsFeatured,
            row.IsAmazing,
            categoryIds,
            instructorIds,
            row.CreatedAt,
            row.CreatedBy,
            row.CreatedByName,
            row.UpdatedAt,
            row.UpdatedBy,
            row.UpdatedByName);
    }

    /// <summary>Reads the active categories and every instructor whose account is still active.</summary>
    public async Task<CourseFormOptionsDto> GetFormOptionsAsync(CancellationToken ct = default)
    {
        const string categoriesSql = $"""
            SELECT "Id", "Name", "ParentId", "DisplayOrder"
            FROM {catalog}."Categories"
            WHERE "IsDeleted" = FALSE AND "IsActive" = TRUE
            ORDER BY "DisplayOrder", "Id";
            """;

        const string instructorsSql = $"""
            SELECT ip."Id", u."FirstName", u."LastName", ip."Specialization"
            FROM {identity}."InstructorProfiles" ip
            JOIN {identity}."Users" u ON u."Id" = ip."UserId"
            WHERE u."IsActive" = TRUE
            ORDER BY u."FirstName", u."LastName";
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);

        var categories = (await connection.QueryAsync<CourseCategoryOptionDto>(categoriesSql))
            .AsList();
        var instructors = (await connection.QueryAsync<CourseInstructorOptionDto>(instructorsSql))
            .AsList();

        return new CourseFormOptionsDto(categories, instructors);
    }

    /// <summary>Returns the subset of the ids that are still active and not soft-deleted.</summary>
    public async Task<IReadOnlySet<long>> GetActiveCategoryIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new HashSet<long>();

        const string sql = $"""
            SELECT "Id"
            FROM {catalog}."Categories"
            WHERE "IsDeleted" = FALSE AND "IsActive" = TRUE AND "Id" = ANY(@ids);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var found = await connection.QueryAsync<long>(
            new CommandDefinition(sql, new { ids = ids.ToArray() }, cancellationToken: ct));

        return found.ToHashSet();
    }

    /// <summary>Returns the subset of instructor profiles whose accounts are still active.</summary>
    public async Task<IReadOnlySet<Guid>> GetInstructorProfileIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new HashSet<Guid>();

        const string sql = $"""
            SELECT ip."Id"
            FROM {identity}."InstructorProfiles" ip
            JOIN {identity}."Users" u ON u."Id" = ip."UserId"
            WHERE u."IsActive" = TRUE AND ip."Id" = ANY(@ids);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var found = await connection.QueryAsync<Guid>(
            new CommandDefinition(sql, new { ids = ids.ToArray() }, cancellationToken: ct));

        return found.ToHashSet();
    }

    #region Helpers

    private static string? BuildPublicUrl(string? storedPath)
        => string.IsNullOrWhiteSpace(storedPath)
            ? null
            : "/uploads/images/" + storedPath.Trim().TrimStart('/');

    private static string? BuildThumbnailUrl(string? storedPath)
        => BuildPublicUrl(storedPath)?.Replace(
            "/originals/", "/thumbnails/", StringComparison.Ordinal);

    private static string EnumName<TEnum>(int value, string fallback) where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), value)
            ? Enum.GetName(typeof(TEnum), value) ?? fallback
            : fallback;

    #endregion

    /// <summary>The flat row of the edit read; the two link lists are read from the same batch.</summary>
    private sealed record CourseDetailsRow(
        long Id,
        string Name,
        string LatinName,
        string ShortDescription,
        string? Description,
        string Slug,
        string? MainImage,
        decimal? Price,
        int DiscountType,
        decimal? DiscountValue,
        bool IsSaleActive,
        int DisplayStatus,
        bool IsFeatured,
        bool IsAmazing,
        DateTime CreatedAt,
        Guid? CreatedBy,
        DateTime UpdatedAt,
        Guid? UpdatedBy,
        string? CreatedByName,
        string? UpdatedByName);
}
