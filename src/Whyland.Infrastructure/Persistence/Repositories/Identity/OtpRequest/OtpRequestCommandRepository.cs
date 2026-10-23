using Whyland.Domain.IRepositories.Identity.OtpRequest;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.OtpRequest;

/// <summary>
/// EF Core command repository for the OtpRequest aggregate, on the shared scoped
/// WhylandDbContext; persistence happens through the Unit of Work (s5.1.4).
/// </summary>
public sealed class OtpRequestCommandRepository(WhylandDbContext context) : IOtpRequestCommandRepository
{
    public async Task AddAsync(Domain.Entities.Identity.OtpRequest request, CancellationToken ct = default)
        => await context.OtpRequests.AddAsync(request, ct);

    public Task<Domain.Entities.Identity.OtpRequest?> GetLatestUsableAsync(string phoneNumber, CancellationToken ct = default)
        => context.OtpRequests
            .Where(x => x.PhoneNumber == phoneNumber
                        && !x.IsUsed
                        && x.ExpiresAt > DateTime.UtcNow
                        && x.AttemptCount < x.MaxAttempts)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Id != default, ct);
}