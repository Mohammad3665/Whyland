using Whyland.Shared.Dtos.Identity.InstructorProfile;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Repositories;

/// <summary>
/// Query-side repository for instructor profiles. Implemented with Dapper for non-tracked reads.
/// </summary>
public interface IInstructorProfileQueryRepository
{
    /// <summary>Returns one page of profiles for the admin table.</summary>
    Task<DataTableResult<InstructorProfileListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);

    /// <summary>Returns one profile with its account data and course count, or null.</summary>
    Task<InstructorProfileDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the profile of one account with its course count, or null.</summary>
    Task<InstructorProfileDetailsDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns active accounts that do not already have an instructor profile.</summary>
    Task<IReadOnlyList<InstructorUserOptionDto>> GetEligibleUsersAsync(CancellationToken ct = default);
}
