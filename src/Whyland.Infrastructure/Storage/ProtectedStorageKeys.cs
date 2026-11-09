using Microsoft.Extensions.Hosting;

using Whyland.Application.Common.Options;

namespace Whyland.Infrastructure.Storage;

/// <summary>
/// Guards for the storage keys of the protected media root. Every key placed in a database column
/// or received over a request is server-generated; these checks keep a hand-edited or copied value
/// from reaching any file outside the directory the key was created for (s8.1.1, s8.5).
/// </summary>
public static class ProtectedStorageKeys
{
    /// <summary>
    /// Whether <paramref name="relativePath"/> addresses a file directly inside
    /// <paramref name="directory"/>. A valid key is exactly the directory segment followed by a
    /// name this system generates (a 32-character hex GUID plus a lowercase extension of one to
    /// five letters); no subdirectories and no traversal are part of a key.
    /// </summary>
    /// <param name="relativePath">The stored key, e.g. <c>videos/0f8a….mp4</c>.</param>
    /// <param name="directory">The area directory the key must live in, e.g. <c>videos</c>.</param>
    public static bool IsValid(string? relativePath, string directory)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || string.IsNullOrWhiteSpace(directory))
            return false;

        var normalized = relativePath.Trim().Replace('\\', '/');

        if (!normalized.StartsWith(directory + '/', StringComparison.OrdinalIgnoreCase))
            return false;

        var fileName = normalized[(directory.Length + 1)..];

        if (fileName.Contains('/'))
            return false;

        var extension = Path.GetExtension(fileName);

        if (extension is not { Length: >= 2 and <= 6 })
            return false;
        if (extension[0] != '.' || !extension[1..].All(char.IsAsciiLetterOrDigit))
            return false;

        var name = Path.GetFileNameWithoutExtension(fileName);

        return name.Length == 32 && name.All(char.IsAsciiHexDigit);
    }

    /// <summary>Resolves the absolute directory a configured relative media root points at.</summary>
    /// <param name="rootPath">The configured root, either absolute or content-root relative.</param>
    /// <param name="environment">The host environment providing the content root.</param>
    public static string ResolveRootPath(string rootPath, IHostEnvironment environment)
    {
        return Path.IsPathRooted(rootPath)
            ? Path.GetFullPath(rootPath)
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, rootPath));
    }
}
