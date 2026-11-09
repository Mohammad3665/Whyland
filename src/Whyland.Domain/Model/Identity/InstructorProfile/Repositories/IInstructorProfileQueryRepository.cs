using Whyland.Shared.Dtos.Identity.InstructorProfile;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Repositories;

/// <summary>
/// Query-side repository for instructor profiles. Implemented with Dapper for non-tracked reads.
/// </summary>
public interface IInstructorProfileQueryRepository
{
    /// <summary>Returns one page of profiles for the admin table.</summary>
    /// <param name="filter">The paging, sorting, and search parameters for the data table.</param>
    /// <param name="conditions">Additional filter conditions to apply, if any.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<DataTableResult<InstructorProfileListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);

    /// <summary>Returns one profile with its account data and course count, or null.</summary>
    /// <param name="id">The profile identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<InstructorProfileDetailsDto?> GetByIdAsync(InstructorProfileId id, CancellationToken ct = default);

    /// <summary>Returns the profile of one account with its course count, or null.</summary>
    /// <param name="userId">The account identifier the profile belongs to.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<InstructorProfileDetailsDto?> GetByUserIdAsync(UserId userId, CancellationToken ct = default);

    /// <summary>Returns active accounts that do not already have an instructor profile.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<InstructorUserOptionDto>> GetEligibleUsersAsync(CancellationToken ct = default);
}