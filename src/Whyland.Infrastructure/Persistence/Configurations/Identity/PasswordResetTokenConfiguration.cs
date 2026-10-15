
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the single-use token used by the "forgot password" flow.
/// </summary>
public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", SchemaConstants.Identity);

        #region Properties

        // Never store the plain token; only its hash (s5.4)
        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        #endregion

        #region Indexes

        // Lookup by user when verifying a reset request (s5.4)
        builder.HasIndex(x => x.UserId);

        #endregion

        #region Relations

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
