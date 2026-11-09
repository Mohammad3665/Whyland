using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Whyland.Application.Common.Abstractions.Video;
using Whyland.Domain.Model.Catalog.Episode.Entities;

namespace Whyland.Infrastructure.Video;

/// <summary>
/// Resolves external video durations from the public metadata of Aparat, YouTube and Vimeo.
/// <para>
/// Only a provider id parsed from an allow-listed host is used to build the request. The original
/// URL is never fetched, so this service cannot be turned into an SSRF probe by entering an
/// arbitrary address in the episode form.
/// </para>
/// </summary>
public sealed class ExternalVideoMetadataService(
    HttpClient httpClient,
    ILogger<ExternalVideoMetadataService> logger) : IExternalVideoMetadataService
{
    private const int MaxResponseBytes = 2 * 1024 * 1024;

    private static readonly Regex YouTubeDuration = new(
        "\"lengthSeconds\"\\s*:\\s*\"?(?<seconds>\\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex YouTubePathId = new(
        "^/(?:embed|shorts|live)/(?<id>[A-Za-z0-9_-]{11})(?:/|$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex VimeoId = new(
        "(?:^|/)(?<id>\\d+)(?:/|$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex AparatHash = new(
        "^/(?:v|embed|video/video/embed/videohash)/(?<hash>[A-Za-z0-9]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public async Task<TimeSpan?> TryGetDurationAsync(string videoUrl, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(videoUrl?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        try
        {
            var seconds = await ResolveSecondsAsync(uri, ct);
            if (seconds is null || seconds <= 0 || seconds > (long)Episode.MaxDuration.TotalSeconds)
                return null;

            return TimeSpan.FromSeconds(seconds.Value);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogInformation("Reading the duration from {Host} timed out.", uri.Host);
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or JsonException or InvalidOperationException)
        {
            logger.LogInformation(ex, "Reading the duration from {Host} failed.", uri.Host);
            return null;
        }
    }

    private async Task<long?> ResolveSecondsAsync(Uri uri, CancellationToken ct)
    {
        var host = uri.Host.TrimEnd('.').ToLowerInvariant();

        if (host is "aparat.com" or "www.aparat.com")
            return await AparatSecondsAsync(uri, ct);

        if (host is "youtu.be" or "www.youtu.be")
        {
            var shortId = uri.AbsolutePath.Trim('/');
            return IsYouTubeId(shortId) ? await YouTubeSecondsAsync(shortId, ct) : null;
        }

        if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "youtube-nocookie.com" or "www.youtube-nocookie.com")
        {
            var id = uri.Query
                .TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .Where(parts => parts.Length == 2 && parts[0] == "v")
                .Select(parts => Uri.UnescapeDataString(parts[1]))
                .FirstOrDefault(IsYouTubeId);

            if (!IsYouTubeId(id))
            {
                var match = YouTubePathId.Match(uri.AbsolutePath);
                id = match.Success ? match.Groups["id"].Value : null;
            }

            return IsYouTubeId(id) ? await YouTubeSecondsAsync(id!, ct) : null;
        }

        if (host is "vimeo.com" or "www.vimeo.com" or "player.vimeo.com")
        {
            var match = VimeoId.Match(uri.AbsolutePath);
            return match.Success ? await VimeoSecondsAsync(match.Groups["id"].Value, ct) : null;
        }

        return null;
    }

    private async Task<long?> AparatSecondsAsync(Uri uri, CancellationToken ct)
    {
        var match = AparatHash.Match(uri.AbsolutePath);
        if (!match.Success)
            return null;

        var hash = match.Groups["hash"].Value;
        var json = await GetStringAsync(
            $"https://www.aparat.com/etc/api/video/videohash/{Uri.EscapeDataString(hash)}",
            ct);

        if (json is null)
            return null;

        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("video", out var video) &&
               video.TryGetProperty("duration", out var duration)
            ? ReadSeconds(duration)
            : null;
    }

    private async Task<long?> YouTubeSecondsAsync(string id, CancellationToken ct)
    {
        var html = await GetStringAsync(
            $"https://www.youtube.com/watch?v={Uri.EscapeDataString(id)}&hl=en",
            ct);

        if (html is null)
            return null;

        var match = YouTubeDuration.Match(html);
        return match.Success && long.TryParse(
            match.Groups["seconds"].Value,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var seconds)
            ? seconds
            : null;
    }

    private async Task<long?> VimeoSecondsAsync(string id, CancellationToken ct)
    {
        var json = await GetStringAsync(
            "https://vimeo.com/api/oembed.json?url=" +
            Uri.EscapeDataString($"https://vimeo.com/{id}"),
            ct);

        if (json is null)
            return null;

        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("duration", out var duration)
            ? ReadSeconds(duration)
            : null;
    }

    private static long? ReadSeconds(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var numeric))
            return numeric;

        if (element.ValueKind == JsonValueKind.String &&
            long.TryParse(
                element.GetString(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var parsed))
            return parsed;

        return null;
    }

    private static bool IsYouTubeId(string? id) =>
        id is { Length: 11 } && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    /// <summary>Reads a bounded response body so a hostile or broken host cannot exhaust memory.</summary>
    private async Task<string?> GetStringAsync(string url, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "The metadata endpoint {Host} answered {StatusCode}.",
                response.RequestMessage?.RequestUri?.Host,
                (int)response.StatusCode);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var buffer = new MemoryStream();
        var rented = ArrayPool<byte>.Shared.Rent(8192);

        try
        {
            while (buffer.Length < MaxResponseBytes)
            {
                var read = await stream.ReadAsync(rented.AsMemory(0, 8192), ct);
                if (read == 0)
                    break;

                buffer.Write(rented, 0, Math.Min(read, MaxResponseBytes - (int)buffer.Length));
            }

            return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented, clearArray: true);
        }
    }
}