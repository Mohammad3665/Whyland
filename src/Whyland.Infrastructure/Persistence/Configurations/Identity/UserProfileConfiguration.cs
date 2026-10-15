
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.NationalCode)
            .HasMaxLength(20);

        builder.Property(x => x.Avatar)
            .HasMaxLength(256);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Province)
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        #endregion
    }
}
