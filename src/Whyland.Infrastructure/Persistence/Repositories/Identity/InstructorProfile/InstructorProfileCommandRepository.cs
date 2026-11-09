using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.InstructorProfile;

/// <summary>
/// EF Core command repository for instructor profiles on the shared scoped DbContext.
/// </summary>
public sealed class InstructorProfileCommandRepository(WhylandDbContext context)
    : IInstructorProfileCommandRepository
{
    public async Task AddAsync(InstructorProfileEntity profile, CancellationToken ct = default)
        => await context.InstructorProfiles.AddAsync(profile, ct);

    public Task<InstructorProfileEntity?> GetTrackedByIdAsync(InstructorProfileId id, CancellationToken ct = default) =>
        context.InstructorProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<InstructorProfileEntity?> GetTrackedByUserIdAsync(UserId userId, CancellationToken ct = default) =>
        context.InstructorProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);
    
}
