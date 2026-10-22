namespace Whyland.Infrastructure.Identity.Services;

/// <summary>
/// A no-op <see cref="ICurrentUserService"/> that always reports an anonymous user.
/// <para>
/// Used as a safe default in contexts where no authenticated user exists
/// (e.g. background jobs, design-time, or before authentication is wired up).
/// </para>
/// </summary>
public class NullCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;

    public bool IsAuthenticated => false;
}
