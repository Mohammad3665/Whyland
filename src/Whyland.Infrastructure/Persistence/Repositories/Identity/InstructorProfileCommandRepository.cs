using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// EF Core command repository for instructor profiles on the shared scoped DbContext.
/// </summary>
public sealed class InstructorProfileCommandRepository(WhylandDbContext context)
    : IInstructorProfileCommandRepository
{
    public async Task AddAsync(InstructorProfileEntity profile, CancellationToken ct = default)
        => await context.InstructorProfiles.AddAsync(profile, ct);

    public Task<InstructorProfileEntity?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = InstructorProfileId.From(id);
        return context.InstructorProfiles.FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    public Task<InstructorProfileEntity?> GetTrackedByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var key = UserId.From(userId);
        return context.InstructorProfiles.FirstOrDefaultAsync(x => x.UserId == key, ct);
    }
}
