namespace Whyland.Application.Common.Interfaces.Identity;

/// <summary>
/// Provides access to information about the currently authenticated user
/// for the duration of the current request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user,
    /// or <c>null</c> if no user is authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is associated
    /// with an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }
}