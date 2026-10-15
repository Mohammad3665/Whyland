namespace Whyland.Application.Common.Abstractions;

/// <summary>
/// Abstraction over password hashing so the Application layer never depends on a
/// concrete hashing implementation (s15.1.1).
/// Passwords must never be stored in plaintext.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password for storage.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext candidate password against a stored hash.
    /// </summary>
    bool Verify(string password, string passwordHash);
}
