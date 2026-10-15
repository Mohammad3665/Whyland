
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.Key)
            .IsUnique();

        #endregion

        #region Relations

        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
