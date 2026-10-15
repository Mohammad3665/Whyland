using Whyland.Application.Common.Abstractions;

namespace Whyland.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hashing implementation of <see cref="IPasswordHasher"/> (s5.1.1/s15.1.1).
/// BCrypt hashes embed their salt and work factor; hashes must be rehashed with a stronger
/// work factor on login if the configured factor has since been increased.
/// </summary>
public class BCryptPasswordHasher(int workFactor = 12) : IPasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor);

    public bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
