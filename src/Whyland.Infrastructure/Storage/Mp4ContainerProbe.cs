namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Reads structural information out of an MP4 container without decoding it: the file start is
/// checked for the <c>ftyp</c> box and the <c>mvhd</c> header inside <c>moov</c> carries the real
/// playback duration (s8.3.2). Only box headers are read, so the probe works cheaply even for
/// very large files.
/// </summary>
internal static class Mp4ContainerProbe
{
    private const int BoxHeaderSize = 8;
    private const int ExtendedSizeSize = 8;

    /// <summary>
    /// Whether the stream carries an MP4 <c>ftyp</c> box near its start. Some encoders put a
    /// <c>free</c>, <c>skip</c> or <c>wide</c> box before it, so the leading top-level boxes are
    /// walked instead of assuming the file starts with <c>ftyp</c>; the magic check is still what
    /// the extension and declared content type cannot fake.
    /// </summary>
    /// <param name="stream">The stream positioned at the start of the file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static async Task<bool> HasFtypAsync(Stream stream, CancellationToken ct)
    {
        // A file whose ftyp is farther away than this is not a layout any supported encoder emits;
        // bounding the scan keeps a malformed upload from forcing a long walk.
        const int MaxScanBytes = 1024 * 1024;

        stream.Position = 0;

        var end = Math.Min(stream.Length, MaxScanBytes);
        var position = 0L;

        while (position + BoxHeaderSize <= end)
        {
            stream.Position = position;

            var header = new byte[BoxHeaderSize];
            var read = await ReadExactlyAsync(stream, header, ct);

            if (read < BoxHeaderSize)
                return false;

            var declaredSize = ReadUInt32Be(header, 0);
            var boxType = System.Text.Encoding.ASCII.GetString(header, 4, 4);

            if (string.Equals(boxType, "ftyp", StringComparison.Ordinal))
                return true;

            long headerSize;
            long bodyLength;

            if (declaredSize == 1)
            {
                var extended = new byte[ExtendedSizeSize];
                read = await ReadExactlyAsync(stream, extended, ct);

                if (read < ExtendedSizeSize)
                    return false;

                var extendedSize = ReadUInt64Be(extended, 0);
                headerSize = BoxHeaderSize + ExtendedSizeSize;

                if (extendedSize < (ulong)headerSize || extendedSize > long.MaxValue)
                    return false;

                bodyLength = (long)extendedSize - headerSize;
            }
            else if (declaredSize == 0)
            {
                headerSize = BoxHeaderSize;
                bodyLength = stream.Length - (position + headerSize);
            }
            else
            {
                if (declaredSize < BoxHeaderSize)
                    return false;

                headerSize = BoxHeaderSize;
                bodyLength = (long)declaredSize - headerSize;
            }

            var next = position + headerSize + bodyLength;

            if (next <= position || next > stream.Length)
                return false;

            position = next;
        }

        return false;
    }

    /// <summary>
    /// Reads the media duration from the first <c>mvhd</c> header found inside the <c>moov</c>
    /// container. Returns null when the container is malformed, incomplete or has no usable
    /// duration; probe failures make the upload unacceptable rather than guessed around.
    /// </summary>
    /// <param name="stream">The stream positioned at the start of the file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static async Task<TimeSpan?> ReadDurationAsync(Stream stream, CancellationToken ct)
    {
        stream.Position = 0;

        var moov = await FindBoxAsync(stream, "moov", stream.Length, null, ct);

        if (moov is null)
            return null;

        var mvhd = await FindBoxAsync(stream, "mvhd", moov.Value.ContentEnd, moov.Value.ContentStart, ct);

        if (mvhd is null)
            return null;

        return await ReadMovieHeaderAsync(stream, mvhd.Value, ct);
    }

    private readonly record struct Box(long ContentStart, long ContentEnd);

    /// <summary>
    /// Walks the box list of one level of the container and returns the box with
    /// <paramref name="type"/>. <paramref name="end"/> bounds the walk; when scanning the top level
    /// it is the file length, directly inside a container it is that container's payload end.
    /// </summary>
    private static async Task<Box?> FindBoxAsync(
        Stream stream,
        string type,
        long end,
        long? start,
        CancellationToken ct)
    {
        var position = start ?? 0;

        while (position + BoxHeaderSize <= end)
        {
            stream.Position = position;

            var header = new byte[BoxHeaderSize];
            var read = await ReadExactlyAsync(stream, header, ct);

            if (read < BoxHeaderSize)
                return null;

            var declaredSize = ReadUInt32Be(header, 0);
            var boxType = System.Text.Encoding.ASCII.GetString(header, 4, 4);

            long headerSize;
            long bodyLength;

            if (declaredSize == 1)
            {
                // A 64-bit size follows the type; it covers the whole box including both headers.
                var extended = new byte[ExtendedSizeSize];
                read = await ReadExactlyAsync(stream, extended, ct);

                if (read < ExtendedSizeSize)
                    return null;

                var extendedSize = ReadUInt64Be(extended, 0);
                headerSize = BoxHeaderSize + ExtendedSizeSize;

                if (extendedSize < (ulong)headerSize || extendedSize > long.MaxValue)
                    return null;

                bodyLength = (long)extendedSize - headerSize;
            }
            else if (declaredSize == 0)
            {
                // A zero size means the box runs to the end of the enclosing level.
                headerSize = BoxHeaderSize;
                bodyLength = end - (position + headerSize);
            }
            else
            {
                if (declaredSize < BoxHeaderSize)
                    return null;

                headerSize = BoxHeaderSize;
                bodyLength = (long)declaredSize - headerSize;
            }

            var bodyStart = position + headerSize;
            var contentEnd = bodyStart + bodyLength;

            if (contentEnd > end || contentEnd < bodyStart)
                return null;

            if (string.Equals(boxType, type, StringComparison.Ordinal))
                return new Box(bodyStart, contentEnd);

            // Only boxes that themselves are containers of the kinds we need are descended into; a
            // flat box is skipped whole by moving to its end.
            position = contentEnd;
        }

        return null;
    }

    /// <summary>Parses the <c>mvhd</c> payload: version decides the width of the timestamp fields.</summary>
    private static async Task<TimeSpan?> ReadMovieHeaderAsync(Stream stream, Box mvhd, CancellationToken ct)
    {
        var payload = new byte[36];
        var available = Math.Min(payload.Length, mvhd.ContentEnd - mvhd.ContentStart);

        stream.Position = mvhd.ContentStart;

        var read = await ReadExactlyAsync(stream, payload.AsMemory(0, (int)available), ct);

        if (read < 8)
            return null;

        var version = payload[0];

        if (version == 0)
        {
            if (read < 20)
                return null;

            var timescale = ReadUInt32Be(payload, 12);
            var duration = ReadUInt32Be(payload, 16);

            return ToDuration(timescale, duration);
        }

        if (version == 1)
        {
            if (read < 32)
                return null;

            var timescale = ReadUInt32Be(payload, 20);
            var duration = ReadUInt64Be(payload, 24);

            return ToDuration(timescale, duration);
        }

        return null;
    }

    private static TimeSpan? ToDuration(ulong timescale, ulong duration)
    {
        if (timescale == 0 || duration == 0)
            return null;

        var seconds = (double)duration / timescale;

        return TimeSpan.FromSeconds(seconds);
    }

    private static uint ReadUInt32Be(byte[] buffer, int offset) =>
        (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) |
               (buffer[offset + 2] << 8) | buffer[offset + 3]);

    private static ulong ReadUInt64Be(byte[] buffer, int offset)
    {
        var high = (ulong)ReadUInt32Be(buffer, offset) << 32;
        var low = ReadUInt32Be(buffer, offset + 4);

        return high | low;
    }

    private static async ValueTask<int> ReadExactlyAsync(Stream stream, Memory<byte> buffer, CancellationToken ct)
    {
        var total = 0;

        while (total < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer.Slice(total), ct);

            if (count == 0)
                break;

            total += count;
        }

        return total;
    }
}