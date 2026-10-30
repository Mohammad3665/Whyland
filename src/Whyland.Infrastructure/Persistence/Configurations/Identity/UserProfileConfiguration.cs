namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the extended customer profile attached to a user one-to-one (s5.2).
/// </summary>
public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.Avatar)
            .HasMaxLength(UserProfile.MaxAvatarLength);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        #endregion
    }
}
