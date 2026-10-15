
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", SchemaConstants.Identity);

        #region Keys

        // Duplicate permission assignments must not be allowed (s6.3)
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        #endregion
    }
}
