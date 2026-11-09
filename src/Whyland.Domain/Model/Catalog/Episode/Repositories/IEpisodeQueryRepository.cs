using Whyland.Shared.Dtos.Catalog.Episode;

namespace Whyland.Domain.Model.Catalog.Episode.Repositories;

/// <summary>
/// Query-side repository for the <see cref="Episode"/> aggregate.
/// Implemented with Dapper for lightweight, non-tracked read queries.
/// </summary>
public interface IEpisodeQueryRepository
{
    /// <summary>
    /// Returns every live episode of a section with its attachment count and video state, ordered by
    /// DisplayOrder.
    /// </summary>
    /// <param name="sectionId">The section whose episodes are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of episodes ordered by DisplayOrder.</returns>
    Task<IReadOnlyList<EpisodeListItemDto>> GetForSectionAsync(CourseSectionId sectionId, CancellationToken ct = default);

    /// <summary>Returns the section header data the episodes page needs, or null when it is gone.</summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The section identifier, owning course and title, or null.</returns>
    Task<EpisodeSectionInfoDto?> GetSectionInfoAsync(CourseSectionId sectionId, CancellationToken ct = default);

    /// <summary>Returns the episode header data the attachment page needs, or null when it is gone.</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The episode identifier, its section, its course and its title, or null.</returns>
    Task<EpisodeHeaderInfoDto?> GetHeaderInfoAsync(EpisodeId episodeId, CancellationToken ct = default);

    /// <summary>
    /// Resolves the playback inputs of one episode: its course, the stored video key, the duration and
    /// the free-preview flag (s7.7.3, s8.1.5).
    /// </summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The playback inputs, or null when no such live episode exists.</returns>
    Task<EpisodePlaybackInfoDto?> GetPlaybackInfoAsync(EpisodeId episodeId, CancellationToken ct = default);

    /// <summary>
    /// Resolves the access inputs of one episode without reading its file path: used for attachment
    /// downloads, which never inherit the free-preview exemption (s8.2.1).
    /// </summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The access inputs, or null when no such live episode exists.</returns>
    Task<EpisodeAccessInfoDto?> GetAccessInfoAsync(EpisodeId episodeId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether the user is entitled to the course through a purchase or an explicit instructor
    /// assignment (s7.7.4.3, s8.5).
    /// </summary>
    /// <param name="courseId">The course being checked.</param>
    /// <param name="userId">The authenticated user's identifier; anonymous callers are refused before this point.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the user owns the course or teaches it.</returns>
    Task<bool> HasCourseAccessAsync(CourseId courseId, UserId userId, CancellationToken ct = default);
}
