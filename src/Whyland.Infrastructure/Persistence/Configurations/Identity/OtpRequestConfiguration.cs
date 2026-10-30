namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the one-time password request entity used by phone-based authentication.
/// </summary>
public sealed class OtpRequestConfiguration : IEntityTypeConfiguration<OtpRequest>
{
    public void Configure(EntityTypeBuilder<OtpRequest> builder)
    {
        builder.ToTable("OtpRequests", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(OtpRequest.MaxPhoneNumberLength);

        // Never store the plain OTP code; only its hash (s5.1.4)
        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(OtpRequest.MaxCodeHashLength);

        builder.Property(x => x.MaxAttempts)
            .HasDefaultValue(5);

        builder.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        // Codes are purpose-scoped: a login code must never be accepted as proof of a phone
        // binding, or the reverse. Rows issued before this column keep the default (Login).
        builder.Property(x => x.Purpose)
            .HasConversion<int>()
            .HasDefaultValue(OtpPurpose.Login);

        #endregion

        #region Indexes

        // Cleanup queries target expired requests by phone number (s5.1.4)
        builder.HasIndex(x => new { x.PhoneNumber, x.ExpiresAt });

        // Every verification and cooldown read takes one number and one purpose.
        builder.HasIndex(x => new { x.PhoneNumber, x.Purpose, x.CreatedAt });

        #endregion
    }
}
