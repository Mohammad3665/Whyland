using Whyland.Application.Common.Abstractions.UnitOfWork;
using Whyland.Application.Common.Interfaces.Identity;

namespace Whyland.Web.Common.Identity.Services;

/// <summary>
/// Resolves the avatar of the signed-in user for the public site header's profile button.
/// Returns null for anonymous visitors or when no avatar has been set, so the layout
/// falls back to the person icon (s5.1.2).
/// </summary>
/// <param name="currentUser">Identifies the signed-in user off the auth cookie.</param>
/// <param name="unitOfWork">Provides the profile read.</param>
public sealed class CurrentUserAvatarService(
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
{
    /// <summary>The avatar path/URL of the signed-in user, or null when unavailable.</summary>
    public async Task<string?> GetAvatarUrlAsync(CancellationToken ct = default)
    {
        if (!currentUser.UserId.HasValue)
            return null;

        return await unitOfWork.UserQueryRepository.GetAvatarAsync(currentUser.UserId.Value, ct);
    }
}
