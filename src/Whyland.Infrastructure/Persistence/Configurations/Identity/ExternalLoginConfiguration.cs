
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the external (social) login link between a user account and an external provider.
/// </summary>
public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProviderKey)
            .IsRequired()
            .HasMaxLength(256);

        #endregion

        #region Keys

        builder.HasKey(x => x.Id);

        #endregion

        #region Indexes

        // A provider must never hand out the same subject twice (s5.3)
        builder.HasIndex(x => new { x.Provider, x.ProviderKey })
            .IsUnique();

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
