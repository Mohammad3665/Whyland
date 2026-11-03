using Whyland.Domain.Model.Identity.OtpRequest.Repositories;

namespace Whyland.Infrastructure.Persistence.Repositories.Identity.OtpRequest;

/// <summary>
/// EF Core command repository for the OtpRequest aggregate, on the shared scoped
/// WhylandDbContext; persistence happens through the Unit of Work (s5.1.4).
/// </summary>
public sealed class OtpRequestCommandRepository(WhylandDbContext context) : IOtpRequestCommandRepository
{
    public async Task AddAsync(Domain.Model.Identity.OtpRequest.Entities.OtpRequest request, CancellationToken ct = default)
        => await context.OtpRequests.AddAsync(request, ct);

    public Task<Domain.Model.Identity.OtpRequest.Entities.OtpRequest?> GetLatestUsableAsync(
        string phoneNumber,
        Domain.Model.Identity.OtpRequest.Enums.OtpPurpose purpose,
        CancellationToken ct = default)
        => context.OtpRequests
            .Where(x => x.PhoneNumber == phoneNumber
                        && x.Purpose == purpose
                        && !x.IsUsed
                        && x.ExpiresAt > DateTime.UtcNow
                        && x.AttemptCount < x.MaxAttempts)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Id != default, ct);

    public Task<Domain.Model.Identity.OtpRequest.Entities.OtpRequest?> GetLatestAsync(
        string phoneNumber,
        Domain.Model.Identity.OtpRequest.Enums.OtpPurpose purpose,
        CancellationToken ct = default)
        => context.OtpRequests
            .Where(x => x.PhoneNumber == phoneNumber
                        && x.Purpose == purpose)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Id != default, ct);
}