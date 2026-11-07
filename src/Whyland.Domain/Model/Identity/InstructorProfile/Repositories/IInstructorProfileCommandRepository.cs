using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Repositories;

/// <summary>
/// Command-side repository for <see cref="InstructorProfileEntity"/>.
/// Persistence goes through <c>IUnitOfWork.SaveAsync()</c>; this interface never saves on its own.
/// </summary>
public interface IInstructorProfileCommandRepository
{
    /// <summary>Stages a new profile; persisted on SaveAsync.</summary>
    Task AddAsync(InstructorProfileEntity profile, CancellationToken ct = default);

    /// <summary>Returns the tracked profile with the given id, or null.</summary>
    Task<InstructorProfileEntity?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the tracked profile that belongs to the given account, or null.</summary>
    Task<InstructorProfileEntity?> GetTrackedByUserIdAsync(Guid userId, CancellationToken ct = default);
}
