namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The per-user watch state of a single episode (s4.7). Stored on the per-user
/// progress entity, never on the episode itself.
/// </summary>
public enum EpisodeWatchStatus
{
    /// <summary>
    /// The user has not started watching the episode.
    /// </summary>
    NotWatched,

    /// <summary>
    /// Playback has started but the episode is not fully watched yet.
    /// </summary>
    InProgress,

    /// <summary>
    /// The user has watched the episode to the end.
    /// </summary>
    Watched,

    /// <summary>
    /// The episode exists but is not yet accessible to the user
    /// (e.g. a later section of a course in sequential order).
    /// </summary>
    Locked,
}
