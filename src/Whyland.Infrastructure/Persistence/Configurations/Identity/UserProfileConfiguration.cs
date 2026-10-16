
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.Avatar)
            .HasMaxLength(256);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        #endregion
    }
}
