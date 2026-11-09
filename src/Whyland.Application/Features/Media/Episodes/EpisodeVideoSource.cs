using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Media.Episodes;

/// <summary>
/// The authorized video of one episode: either an open file inside protected storage, or an external
/// link the caller is allowed to follow. Exactly one of the two is set.
/// </summary>
/// <param name="File">The open protected file, when the video is hosted by this platform.</param>
/// <param name="ExternalUrl">The external http(s) link, when the episode uses one.</param>
public sealed record EpisodeVideoSource(VideoFileStream? File, string? ExternalUrl)
{
    /// <summary>Whether <see cref="ExternalUrl"/> is the source of this video.</summary>
    public bool IsExternal => !string.IsNullOrWhiteSpace(ExternalUrl);
}
