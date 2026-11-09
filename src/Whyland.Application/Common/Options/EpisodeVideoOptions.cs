namespace Whyland.Application.Common.Options;

/// <summary>
/// Limits and layout of the protected episode-video storage (s7.7, s8.3.2).
/// Bound from the <c>EpisodeVideo</c> configuration section.
/// </summary>
public sealed class EpisodeVideoOptions
{
    /// <summary>Configuration section this class binds from.</summary>
    public const string SectionName = "EpisodeVideo";

    /// <summary>
    /// Root of the protected media storage, relative to the content root. Static serving of this
    /// root is blocked, so a file can only be reached through the checked endpoints (s8.1.1).
    /// </summary>
    public string RootPath { get; set; } = "wwwroot/uploads/media";

    /// <summary>Directory segment the episode videos are stored in, inside the root.</summary>
    public string VideoDirectory { get; set; } = "videos";

    /// <summary>The largest single video file, in bytes (500 MB by default).</summary>
    public long MaxFileSizeBytes { get; set; } = 500L * 1024 * 1024;

    /// <summary>The longest video the platform accepts, in seconds (24h, mirroring <c>Episode.MaxDuration</c>).</summary>
    public int MaxDurationSeconds { get; set; } = 24 * 60 * 60;
}
