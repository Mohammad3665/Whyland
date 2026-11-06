namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Strongly-typed options for local image storage, bound from the "FileStorage" section.
/// <para>
/// Every limit lives here rather than in the validator so a deployment can tighten them without a
/// code change (s8.3.1, s15.1.6).
/// </para>
/// </summary>
public sealed class LocalImageStorageOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Storage root. Relative paths resolve against the host content root, so the default keeps the
    /// files under the served wwwroot folder (s8.3.2).
    /// </summary>
    public string RootPath { get; set; } = "wwwroot/uploads/images";

    /// <summary>Sub-directory holding the full-size images.</summary>
    public string OriginalDirectory { get; set; } = "originals";

    /// <summary>Sub-directory holding the generated thumbnails.</summary>
    public string ThumbnailDirectory { get; set; } = "thumbnails";

    /// <summary>
    /// Maps the logical area key used by callers to its directory name. An area that is not listed
    /// here is refused, so a caller can never choose an arbitrary path.
    /// </summary>
    public Dictionary<string, string> Areas { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Largest upload accepted, in bytes.</summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Largest accepted source width, in pixels.</summary>
    public int MaxImageWidth { get; set; } = 10_000;

    /// <summary>Largest accepted source height, in pixels.</summary>
    public int MaxImageHeight { get; set; } = 10_000;

    /// <summary>Largest accepted source area, in pixels; guards against decompression bombs.</summary>
    public long MaxImagePixels { get; set; } = 25_000_000;

    /// <summary>Largest stored image after processing, in bytes.</summary>
    public long MaxOutputImageSizeBytes { get; set; } = 300_000;

    /// <summary>Largest stored thumbnail, in bytes.</summary>
    public long MaxThumbnailSizeBytes { get; set; } = 80_000;

    /// <summary>Largest dimension of a stored image; larger sources are resized down.</summary>
    public int MaxOriginalDimension { get; set; } = 1920;

    /// <summary>Largest dimension of a thumbnail.</summary>
    public int ThumbnailMaxDimension { get; set; } = 320;

    /// <summary>WebP quality used when <see cref="Application.Common.Abstractions.Storage.ImageStorageSaveOptions.OptimizeImage"/> is set.</summary>
    public int WebPQuality { get; set; } = 82;

    /// <summary>JPEG quality used when the source cannot be converted and optimization is requested.</summary>
    public int JpegQuality { get; set; } = 85;

    /// <summary>How many images may be encoded at the same time.</summary>
    public int MaxConcurrentOperations { get; set; } = 2;
}
