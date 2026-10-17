using Whyland.Application.Common.Interfaces.Identity;

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
    /// <summary>
    /// Always <c>null</c> because no real user is associated with this service.
    /// </summary>
    public Guid? UserId => null;

    /// <summary>
    /// Always <c>false</c> because this service represents an anonymous context.
    /// </summary>
    public bool IsAuthenticated => false;
}
