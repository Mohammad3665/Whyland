namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps a system role that can be assigned both users and permissions (s6.3/s6.4).
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Role.MaxNameLength);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Role.MaxTitleLength);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.Name)
            .IsUnique();

        #endregion

        #region Relations

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
