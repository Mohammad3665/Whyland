namespace Whyland.Application.Common.Abstractions.Video;

/// <summary>
/// Reads the duration of a video hosted by a supported external provider (Aparat, YouTube or
/// Vimeo). The adapter only contacts the provider's own metadata endpoint for a parsed public id;
/// it never fetches an arbitrary URL supplied by a caller.
/// </summary>
public interface IExternalVideoMetadataService
{
    /// <summary>
    /// Tries to read the duration of one external video. A provider that is unsupported, unreachable
    /// or does not expose a duration returns <c>null</c>; the link itself can still be saved.
    /// </summary>
    /// <param name="videoUrl">The absolute http(s) link entered by the operator.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The duration, or <c>null</c> when it cannot be resolved.</returns>
    Task<TimeSpan?> TryGetDurationAsync(string videoUrl, CancellationToken ct = default);
}