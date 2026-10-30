namespace Whyland.Domain.Entities.Learning;

/// <summary>
/// The per-user playback progress of a single episode, supporting resume and completion tracking (s4.7).
/// </summary>
public class UserEpisodeProgress
{
    #region Properties

    /// <summary>
    /// The user whose progress is tracked.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The watched episode.
    /// </summary>
    public long EpisodeId { get; set; }

    /// <summary>
    /// Aggregate watch state of the episode for this user.
    /// </summary>
    public EpisodeWatchStatus Status { get; set; }

    /// <summary>
    /// Last playback position used to resume the episode (s4.7.3).
    /// </summary>
    public TimeSpan? LastWatchedPosition { get; set; }

    /// <summary>
    /// UTC time the user first started watching this episode.
    /// </summary>
    public DateTime? FirstWatchedAt { get; set; }

    /// <summary>
    /// UTC time the user first fully watched the episode.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// UTC time of the user's most recent watch activity.
    /// </summary>
    public DateTime? LastWatchedAt { get; set; }

    /// <summary>
    /// UTC time the progress record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC time of the latest progress update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The tracked user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The watched episode.
    /// </summary>
    public Episode Episode { get; set; } = null!;

    #endregion
}
