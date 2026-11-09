using Whyland.Domain.Model.Identity.OtpRequest.Enums;

using OtpRequestEntity = Whyland.Domain.Model.Identity.OtpRequest.Entities.OtpRequest;

namespace Whyland.Domain.Model.Identity.OtpRequest.Repositories;

/// <summary>
/// Command-side repository for the OtpRequest aggregate.
/// Backed by EF Core (change tracking); persistence goes through
/// IUnitOfWork.SaveAsync() — this interface never saves on its own (s5.1.4).
/// </summary>
public interface IOtpRequestCommandRepository
{
    /// <summary>Stages a new OTP request; persisted on SaveAsync.</summary>
    /// <param name="request">The OTP request entity to add.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task AddAsync(OtpRequestEntity request, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent *usable* OTP of one purpose for a phone number: not used,
    /// not expired, and with attempts remaining. Tracked, so the caller can
    /// mutate AttemptCount / IsUsed and persist via SaveAsync. Null if none.
    /// </summary>
    /// <param name="phoneNumber">The phone number the OTP was issued to.</param>
    /// <param name="purpose">The OTP purpose (e.g. login, binding) used to scope the lookup.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<OtpRequestEntity?> GetLatestUsableAsync(
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the newest OTP row of one purpose for a phone number, used or expired. Used for the
    /// resend cooldown, which must survive a code that was invalidated because it never delivered.
    /// A binding code must not throttle a login code for the same number, hence the per-purpose read.
    /// </summary>
    /// <param name="phoneNumber">The phone number whose latest OTP row is read.</param>
    /// <param name="purpose">The OTP purpose (e.g. login, binding) used to scope the lookup.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<OtpRequestEntity?> GetLatestAsync(
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken ct = default);
}