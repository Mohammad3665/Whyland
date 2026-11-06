namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Raised while an upload is being copied when the bytes read exceed the configured ceiling.
/// It never leaves this layer: the storage service maps it to a boundary error.
/// </summary>
/// <param name="maxBytes">The configured limit, kept for the log entry.</param>
internal sealed class ImageStorageFileTooLargeException(long maxBytes)
    : Exception($"The uploaded image exceeds the {maxBytes} byte limit.")
{
    /// <summary>The configured size limit, in bytes.</summary>
    public long MaxBytes { get; } = maxBytes;
}
