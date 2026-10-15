
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
            .HasMaxLength(20);

        // Never store the plain OTP code; only its hash (s5.1.4)
        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.MaxAttempts)
            .HasDefaultValue(5);

        builder.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        #endregion

        #region Indexes

        // Cleanup queries target expired requests by phone number (s5.1.4)
        builder.HasIndex(x => new { x.PhoneNumber, x.ExpiresAt });

        #endregion
    }
}
