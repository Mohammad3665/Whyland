namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// An open handle on a stored media file, shared by the video and attachment storage contracts.
/// The stream is positioned at the start of the file; the streaming response owns it and must
/// dispose it.
/// </summary>
/// <param name="Content">The opened read-only stream.</param>
/// <param name="Length">Total length of the file in bytes, the boundary the Range range checks use.</param>
public sealed record VideoFileStream(Stream Content, long Length) : IDisposable
{
    /// <inheritdoc />
    public void Dispose() => Content.Dispose();
}
