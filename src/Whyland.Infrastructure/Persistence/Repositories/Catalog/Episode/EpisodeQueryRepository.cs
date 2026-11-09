using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;
using SectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.Episode;
using Whyland.Domain.Model.Catalog.Episode.Repositories;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Episode;

/// <summary>
/// Query-side repository for the <see cref="Episode"/> aggregate. Implemented with the shared Dapper
/// query builder for single-table reads and one hand-written statement for the course-join reads, so
/// the access decision never has to guess at the owning course (s7.7.3, s8.1.5).
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class EpisodeQueryRepository(NpgsqlDataSource dataSource) : IEpisodeQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;
    private const string learning = SchemaConstants.Learning;
    private const string identity = SchemaConstants.Identity;

    /// <summary>The quoted duration column of the root table, read as whole seconds.</summary>
    private static readonly string DurationSecondsColumn = $"""
        (EXTRACT(EPOCH FROM {DapperFluentQueryBuilder<EpisodeEntity>.RootColumn(nameof(EpisodeEntity.Duration))}))::int
        """;

    /// <summary>Whether a stored video file is attached, resolved in the database (s8.1.7).</summary>
    private static readonly string HasVideoColumn = $"""
        ({DapperFluentQueryBuilder<EpisodeEntity>.RootColumn(nameof(EpisodeEntity.Video))} IS NOT NULL)
        """;

    /// <summary>
    /// Returns the external video link to the admin list, or null when the episode has no video or a
    /// protected file. The protected storage key never leaves the server (s8.1.7).
    /// </summary>
    private static readonly string VideoUrlColumn = $"""
        (CASE WHEN {DapperFluentQueryBuilder<EpisodeEntity>.RootColumn(nameof(EpisodeEntity.Video))} LIKE 'http%'
              THEN {DapperFluentQueryBuilder<EpisodeEntity>.RootColumn(nameof(EpisodeEntity.Video))}
              ELSE NULL END)
        """;

    /// <summary>
    /// Counts the live attachments of an episode on every read, as s7.8 requires; no denormalized
    /// counter has to be kept in sync when attachments arrive or leave.
    /// </summary>
    private static readonly string AttachmentCountColumn = $"""
        (SELECT COUNT(*)::int
         FROM {catalog}."EpisodeAttachments" a
         WHERE a."EpisodeId" = {DapperFluentQueryBuilder<EpisodeEntity>.RootColumn(nameof(EpisodeEntity.Id))}
           AND a."IsDeleted" = FALSE)
        """;

    /// <summary>
    /// Returns the live episodes of one section with their video state and attachment count, ordered
    /// by DisplayOrder and then by Id so two rows that share a position still come back in a stable
    /// order.
    /// </summary>
    /// <param name="sectionId">The section whose episodes are read.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<IReadOnlyList<EpisodeListItemDto>> GetForSectionAsync(
        CourseSectionId sectionId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<EpisodeEntity>.Create(
                $"{catalog}.Episodes",
                x => x.Id,
                x => x.SectionId,
                x => x.Title,
                x => x.Description,
                x => x.DisplayOrder,
                x => x.IsFree,
                x => x.CreatedAt)
            .SelectExpression(DurationSecondsColumn, nameof(EpisodeListItemDto.DurationSeconds))
            .SelectExpression(HasVideoColumn, nameof(EpisodeListItemDto.HasVideo))
            .SelectExpression(AttachmentCountColumn, nameof(EpisodeListItemDto.AttachmentCount))
            .SelectExpression(VideoUrlColumn, nameof(EpisodeListItemDto.VideoUrl))
            .Where(x => x.SectionId, sectionId.Value)
            .OrderBy(x => x.DisplayOrder)
            .TieBreakBy(x => x.Id)
            .GetAllAsync<EpisodeListItemDto>(dataSource, ct);

    /// <summary>Returns the section header data the episodes page needs, or null when it is gone.</summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeSectionInfoDto?> GetSectionInfoAsync(
        CourseSectionId sectionId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<SectionEntity>.Create(
                $"{catalog}.CourseSections",
                x => x.Id,
                x => x.CourseId,
                x => x.Title)
            .Where(x => x.Id, sectionId.Value)
            .GetFirstOrDefaultAsync<EpisodeSectionInfoDto>(dataSource, ct);

    /// <summary>Returns the episode header data the attachment page needs, or null when it is gone.</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeHeaderInfoDto?> GetHeaderInfoAsync(EpisodeId episodeId, CancellationToken ct = default)
        // The output order must match EpisodeHeaderInfoDto's constructor: Dapper materializes a
        // positional record by column type, so Title before CourseId throws instead of mapping by name.
        => DapperFluentQueryBuilder<EpisodeEntity>.Create(
                $"{catalog}.Episodes",
                x => x.Id,
                x => x.SectionId)
            .Join<SectionEntity>($"{catalog}.CourseSections", x => x.SectionId, s => s.Id)
            .SelectFrom<SectionEntity>(s => s.CourseId, nameof(EpisodeHeaderInfoDto.CourseId))
            .Select(x => x.Title)
            .Where(x => x.Id, episodeId.Value)
            .GetFirstOrDefaultAsync<EpisodeHeaderInfoDto>(dataSource, ct);

    /// <summary>
    /// Resolves the playback inputs of one episode: its course, the stored video key, the duration
    /// and the free-preview flag (s7.7.3, s8.1.5). A live episode only is returned; soft-deleted rows
    /// are invisible even to a privileged caller.
    /// </summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodePlaybackInfoDto?> GetPlaybackInfoAsync(
        EpisodeId episodeId,
        CancellationToken ct = default)
        // Keep the select order aligned with EpisodePlaybackInfoDto's constructor; Dapper matches
        // positional record parameters by the reader's column order and types.
        => DapperFluentQueryBuilder<EpisodeEntity>.Create(
                $"{catalog}.Episodes",
                x => x.Id)
            .Join<SectionEntity>($"{catalog}.CourseSections", x => x.SectionId, s => s.Id)
            .Join<SectionEntity, CourseEntity>($"{catalog}.Courses", s => s.CourseId, c => c.Id)
            .SelectFrom<SectionEntity>(s => s.CourseId, nameof(EpisodePlaybackInfoDto.CourseId))
            .Select(x => x.Video)
            .SelectExpression(DurationSecondsColumn, nameof(EpisodePlaybackInfoDto.DurationSeconds))
            .Select(x => x.IsFree)
            .SelectFrom<CourseEntity>(c => c.DisplayStatus, nameof(EpisodePlaybackInfoDto.CourseDisplayStatus))
            .Where(x => x.Id, episodeId.Value)
            .WhereFrom<SectionEntity>(s => s.IsDeleted, false)
            .WhereFrom<CourseEntity>(c => c.IsDeleted, false)
            .GetFirstOrDefaultAsync<EpisodePlaybackInfoDto>(dataSource, ct);

    /// <summary>
    /// Resolves the access inputs of one episode without reading its file path: used for attachment
    /// downloads, which never inherit the free-preview exemption (s8.2.1).
    /// </summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeAccessInfoDto?> GetAccessInfoAsync(
        EpisodeId episodeId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<EpisodeEntity>.Create(
                $"{catalog}.Episodes",
                x => x.Id)
            .Join<SectionEntity>($"{catalog}.CourseSections", x => x.SectionId, s => s.Id)
            .Join<SectionEntity, CourseEntity>($"{catalog}.Courses", s => s.CourseId, c => c.Id)
            .SelectFrom<SectionEntity>(s => s.CourseId, nameof(EpisodeAccessInfoDto.CourseId))
            .Select(x => x.IsFree)
            .SelectFrom<CourseEntity>(c => c.DisplayStatus, nameof(EpisodeAccessInfoDto.CourseDisplayStatus))
            .Where(x => x.Id, episodeId.Value)
            .WhereFrom<SectionEntity>(s => s.IsDeleted, false)
            .WhereFrom<CourseEntity>(c => c.IsDeleted, false)
            .GetFirstOrDefaultAsync<EpisodeAccessInfoDto>(dataSource, ct);

    /// <summary>
    /// Checks whether the user is entitled to the course through a purchase or an explicit instructor
    /// assignment (s7.7.4.3, s8.5). The instructor route runs through the instructor profile, whose
    /// row carries the user id.
    /// </summary>
    /// <param name="courseId">The course being checked.</param>
    /// <param name="userId">The authenticated user's identifier; anonymous callers are refused before this point.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<bool> HasCourseAccessAsync(
        CourseId courseId,
        UserId userId,
        CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT EXISTS(
                       SELECT 1
                       FROM {learning}."UserCourses" uc
                       WHERE uc."CourseId" = @CourseId
                         AND uc."UserId" = @UserId)
                   OR EXISTS(
                       SELECT 1
                       FROM {catalog}."CourseInstructors" ci
                       JOIN {identity}."InstructorProfiles" ip ON ip."Id" = ci."InstructorProfileId"
                       WHERE ci."CourseId" = @CourseId
                         AND ip."UserId" = @UserId)
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { CourseId = courseId.Value, UserId = userId.Value },
                cancellationToken: ct));
    }
}
