using Whyland.Domain.Model.Catalog.Course.Repositories;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.Course;

using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;
using CourseCategoryEntity = Whyland.Domain.Model.Catalog.CourseCategory.Entities.CourseCategory;
using CourseInstructorEntity = Whyland.Domain.Model.Catalog.CourseInstructor.Entities.CourseInstructor;
using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;
using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Course;

/// <summary>
/// Query-side repository for the <see cref="CourseEntity"/> aggregate.
/// Implemented with the shared Dapper query builder, so every statement is assembled from typed
/// properties and one engine instead of hand-written SQL text.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class CourseQueryRepository(NpgsqlDataSource dataSource) : ICourseQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;
    private const string identity = SchemaConstants.Identity;

    /// <summary>Alias of the join that resolves the account that created the row.</summary>
    private const string CreatedByAlias = "cu";

    /// <summary>Alias of the join that resolves the account that edited the row last.</summary>
    private const string UpdatedByAlias = "uu";

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
            SqlDataTableSpec.ColumnsOf<CourseEntity>(
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
            nameof(CourseEntity.Name),
            nameof(CourseEntity.LatinName),
            nameof(CourseEntity.Slug),
        ]);

    /// <summary>
    /// Runs one page of the admin listing. The declarative table definition above is the same engine
    /// the builder uses, so paging, search, sorting and the membership filters stay consistent with
    /// every other query.
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="conditions">
    /// Generic field/operator/value conditions from the picker plus the two explicit filters
    /// (<c>CategoryId</c> and <c>InstructorProfileId</c>), ANDed together. Each name is resolved
    /// against the spec's whitelist, so an unknown one is dropped instead of reaching the SQL.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public Task<DataTableResult<CourseListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<CourseListItemDto>(dataSource, filter, conditions, ct);

    /// <summary>
    /// Reads the editable course, its selected category ids and its selected instructor ids over
    /// one pooled connection. A missing row returns null before the two link lists are read.
    /// </summary>
    /// <param name="id">Course identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<CourseDetailsDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        var row = await DapperFluentQueryBuilder<CourseEntity>.Create(
                $"{catalog}.Courses",
                x => x.Id,
                x => x.Name,
                x => x.LatinName,
                x => x.ShortDescription,
                x => x.Description,
                x => x.Slug,
                x => x.MainImage,
                x => x.Price,
                x => x.DiscountType,
                x => x.DiscountValue,
                x => x.IsSaleActive,
                x => x.DisplayStatus,
                x => x.IsFeatured,
                x => x.IsAmazing,
                x => x.CreatedAt,
                x => x.CreatedBy,
                x => x.UpdatedAt,
                x => x.UpdatedBy)
            .LeftJoin<UserEntity>($"{identity}.Users", x => x.CreatedBy, u => u.Id, CreatedByAlias)
            .Builder
            .SelectExpression(
                $"{CreatedByAlias}.\"FirstName\" || ' ' || {CreatedByAlias}.\"LastName\"",
                nameof(CourseDetailsDto.CreatedByName))
            .LeftJoin<UserEntity>($"{identity}.Users", x => x.UpdatedBy, u => u.Id, UpdatedByAlias)
            .Builder
            .SelectExpression(
                $"{UpdatedByAlias}.\"FirstName\" || ' ' || {UpdatedByAlias}.\"LastName\"",
                nameof(CourseDetailsDto.UpdatedByName))
            .Where(x => x.Id, id)
            .GetFirstOrDefaultAsync<CourseDetailsRow>(connection, ct);

        if (row is null)
            return null;

        var categoryIds = (await DapperFluentQueryBuilder<CourseCategoryEntity>
                .Create($"{catalog}.CourseCategories", x => x.CategoryId)
                .Where(x => x.CourseId, id)
                .GetAllAsync<long>(connection, ct))
            .ToArray();

        var instructorIds = (await DapperFluentQueryBuilder<CourseInstructorEntity>
                .Create($"{catalog}.CourseInstructors", x => x.InstructorProfileId)
                .Where(x => x.CourseId, id)
                .GetAllAsync<Guid>(connection, ct))
            .ToArray();

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

    /// <summary>
    /// Reads the active categories and every instructor whose account is still active, over one
    /// pooled connection.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task<CourseFormOptionsDto> GetFormOptionsAsync(CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        var categories = await DapperFluentQueryBuilder<CategoryEntity>.Create(
                $"{catalog}.Categories",
                x => x.Id,
                x => x.Name,
                x => x.ParentId,
                x => x.DisplayOrder)
            .Where(x => x.IsActive, true)
            .OrderBy(x => x.DisplayOrder)
            .TieBreakBy(x => x.Id)
            .GetAllAsync<CourseCategoryOptionDto>(connection, ct);

        var instructors = await DapperFluentQueryBuilder<InstructorProfileEntity>.Create(
                $"{identity}.InstructorProfiles",
                x => x.Id,
                x => x.Specialization)
            .Join<UserEntity>($"{identity}.Users", x => x.UserId, u => u.Id)
            .SelectFrom<UserEntity>(u => u.FirstName)
            .SelectFrom<UserEntity>(u => u.LastName)
            .WhereFrom<UserEntity>(u => u.IsActive, true)
            .SortableFrom<UserEntity>(u => u.FirstName)
            .SortableFrom<UserEntity>(u => u.LastName)
            .OrderBy(nameof(UserEntity.FirstName))
            .TieBreakBy(x => x.Id)
            .GetAllAsync<CourseInstructorOptionDto>(connection, ct);

        return new CourseFormOptionsDto(categories, instructors);
    }

    /// <summary>Returns the subset of the ids that are still active and not soft-deleted.</summary>
    /// <param name="ids">Category identifiers sent by the form.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<IReadOnlySet<long>> GetActiveCategoryIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new HashSet<long>();

        var found = await DapperFluentQueryBuilder<CategoryEntity>
            .Create($"{catalog}.Categories", x => x.Id)
            .WhereIn(x => x.Id, ids.Cast<object?>())
            .Where(x => x.IsActive, true)
            .GetAllAsync<long>(dataSource, ct);

        return found.ToHashSet();
    }

    /// <summary>Returns the subset of instructor profiles whose accounts are still active.</summary>
    /// <param name="ids">Instructor profile identifiers sent by the form.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<IReadOnlySet<Guid>> GetInstructorProfileIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new HashSet<Guid>();

        var found = await DapperFluentQueryBuilder<InstructorProfileEntity>
            .Create($"{identity}.InstructorProfiles", x => x.Id)
            .Join<UserEntity>($"{identity}.Users", x => x.UserId, u => u.Id)
            .WhereFrom<UserEntity>(u => u.IsActive, true)
            .WhereIn(x => x.Id, ids.Cast<object?>())
            .GetAllAsync<Guid>(dataSource, ct);

        return found.ToHashSet();
    }

    #region Helpers

    /// <summary>Builds the public URL of a stored relative image path, or null when there is none.</summary>
    /// <param name="storedPath">The relative path stored on the course row.</param>
    private static string? BuildPublicUrl(string? storedPath)
        => string.IsNullOrWhiteSpace(storedPath)
            ? null
            : "/uploads/images/" + storedPath.Trim().TrimStart('/');

    /// <summary>Builds the thumbnail URL next to a stored original, or null when there is none.</summary>
    /// <param name="storedPath">The relative path stored on the course row.</param>
    private static string? BuildThumbnailUrl(string? storedPath)
        => BuildPublicUrl(storedPath)?.Replace(
            "/originals/", "/thumbnails/", StringComparison.Ordinal);

    /// <summary>Resolves the stored numeric enum value back to its name, or the fallback.</summary>
    /// <typeparam name="TEnum">The enum the column stores.</typeparam>
    /// <param name="value">The numeric value read from the row.</param>
    /// <param name="fallback">Name used when the stored number is not a defined value.</param>
    private static string EnumName<TEnum>(int value, string fallback) where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), value)
            ? Enum.GetName(typeof(TEnum), value) ?? fallback
            : fallback;

    #endregion

    /// <summary>The flat row of the edit read; the two link lists are read from the same connection.</summary>
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
