using SixLabors.ImageSharp;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// A decoded image plus the format proven by the file signature. The source dimensions are those of
/// the file before decoding and resizing, so the service can decide whether a resize is needed.
/// The decoded image is owned by this instance until it is disposed.
/// </summary>
internal sealed class ValidatedImage(Image image, ImageFileFormat format, int sourceWidth, int sourceHeight) : IDisposable
{
    public Image Image { get; } = image;

    public ImageFileFormat Format { get; } = format;

    /// <summary>Width of the source file before decoding and resizing.</summary>
    public int SourceWidth { get; } = sourceWidth;

    /// <summary>Height of the source file before decoding and resizing.</summary>
    public int SourceHeight { get; } = sourceHeight;

    public void Dispose() => Image.Dispose();
}
