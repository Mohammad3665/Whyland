namespace Whyland.Application.Common.Abstractions.Storage;

/// <summary>
/// Per-upload choices; the size, format and concurrency limits stay in configuration.
/// </summary>
public sealed record ImageStorageSaveOptions
{
    /// <summary>When true, a smaller thumbnail is written next to the image.</summary>
    public bool GenerateThumbnail { get; init; }

    /// <summary>When true, the encoder quality is tuned to keep the file small.</summary>
    public bool OptimizeImage { get; init; } = true;

    /// <summary>When true, the stored image is re-encoded as WebP.</summary>
    public bool ConvertToWebP { get; init; } = true;
}
