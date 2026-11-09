using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Repositories;

/// <summary>
/// Command-side repository for <see cref="InstructorProfileEntity"/>.
/// Persistence goes through <c>IUnitOfWork.SaveAsync()</c>; this interface never saves on its own.
/// </summary>
public interface IInstructorProfileCommandRepository
{
    /// <summary>Stages a new profile; persisted on SaveAsync.</summary>
    /// <param name="profile">The profile entity to add.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task AddAsync(InstructorProfileEntity profile, CancellationToken ct = default);

    /// <summary>Returns the tracked profile with the given id, or null.</summary>
    /// <param name="id">The profile identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<InstructorProfileEntity?> GetTrackedByIdAsync(InstructorProfileId id, CancellationToken ct = default);

    /// <summary>Returns the tracked profile that belongs to the given account, or null.</summary>
    /// <param name="userId">The account identifier the profile belongs to.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<InstructorProfileEntity?> GetTrackedByUserIdAsync(UserId userId, CancellationToken ct = default);
}