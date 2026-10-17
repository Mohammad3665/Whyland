
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the application user with its unique credentials and authentication relations (s5.1).
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        // Unique when present (s5.1.1) - partial unique indexes for nullable columns in PostgreSQL
        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter($"\"{nameof(User.PhoneNumber)}\" IS NOT NULL AND \"{nameof(User.IsDeleted)}\" = FALSE");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter($"\"{nameof(User.Email)}\" IS NOT NULL AND \"{nameof(User.IsDeleted)}\" = FALSE");

        builder.HasIndex(x => x.IsActive);

        #endregion

        #region Relations

        builder.HasOne(x => x.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InstructorProfile)
            .WithOne(i => i.User)
            .HasForeignKey<InstructorProfile>(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
