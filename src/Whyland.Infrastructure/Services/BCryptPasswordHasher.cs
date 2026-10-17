using Whyland.Application.Common.Abstractions;

namespace Whyland.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hashing implementation of <see cref="IPasswordHasher"/> (s5.1.1/s15.1.1).
/// BCrypt hashes embed their salt and work factor; hashes must be rehashed with a stronger
/// work factor on login if the configured factor has since been increased.
/// </summary>
public class BCryptPasswordHasher(int workFactor = 12) : IPasswordHasher
{
    /// <summary>
    /// Hashes a password with BCrypt at the configured work factor; the salt is embedded in the hash.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The BCrypt hash to store.</returns>
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor);

    /// <summary>
    /// Verifies a plaintext password against a stored BCrypt hash.
    /// </summary>
    /// <param name="password">The plaintext password to check.</param>
    /// <param name="passwordHash">The stored hash to compare against.</param>
    /// <returns><c>true</c> when the password matches; otherwise <c>false</c>.</returns>
    public bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
