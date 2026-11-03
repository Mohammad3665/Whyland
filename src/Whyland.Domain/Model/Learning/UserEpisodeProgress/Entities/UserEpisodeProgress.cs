namespace Whyland.Domain.Model.Learning.UserEpisodeProgress.Entities;

/// <summary>
/// The per-user playback progress of a single episode, supporting resume and completion tracking (s4.7).
/// </summary>
public class UserEpisodeProgress
{
    #region Properties

    /// <summary>
    /// The user whose progress is tracked.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The watched episode.
    /// </summary>
    public EpisodeId EpisodeId { get; private set; }

    /// <summary>
    /// Aggregate watch state of the episode for this user.
    /// </summary>
    public EpisodeWatchStatus Status { get; private set; }

    /// <summary>
    /// Last playback position used to resume the episode (s4.7.3).
    /// </summary>
    public TimeSpan? LastWatchedPosition { get; private set; }

    /// <summary>
    /// UTC time the user first started watching this episode.
    /// </summary>
    public DateTime? FirstWatchedAt { get; private set; }

    /// <summary>
    /// UTC time the user first fully watched the episode.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// UTC time of the user's most recent watch activity.
    /// </summary>
    public DateTime? LastWatchedAt { get; private set; }

    /// <summary>
    /// UTC time the progress record was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC time of the latest progress update.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The tracked user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The watched episode.
    /// </summary>
    public Episode Episode { get; private set; } = null!;

    #endregion
}
