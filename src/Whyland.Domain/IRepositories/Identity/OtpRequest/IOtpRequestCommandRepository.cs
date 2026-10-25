namespace Whyland.Domain.IRepositories.Identity.OtpRequest;

/// <summary>
/// Command-side repository for the OtpRequest aggregate.
/// Backed by EF Core (change tracking); persistence goes through
/// IUnitOfWork.SaveAsync() — this interface never saves on its own (s5.1.4).
/// </summary>
public interface IOtpRequestCommandRepository
{
    /// <summary>Stages a new OTP request; persisted on SaveAsync.</summary>
    Task AddAsync(Entities.Identity.OtpRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent *usable* OTP for a phone number: not used,
    /// not expired, and with attempts remaining. Tracked, so the caller can
    /// mutate AttemptCount / IsUsed and persist via SaveAsync. Null if none.
    /// </summary>
    Task<Entities.Identity.OtpRequest?> GetLatestUsableAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Returns the newest OTP row for a phone number, used or expired. Used for the resend
    /// cooldown, which must survive a code that was invalidated because it never delivered.
    /// </summary>
    Task<Entities.Identity.OtpRequest?> GetLatestAsync(string phoneNumber, CancellationToken ct = default);
}