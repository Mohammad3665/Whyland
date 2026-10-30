namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the many-to-many assignment of roles to users (s6.4).
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", SchemaConstants.Identity);

        #region Keys

        // Duplicate user/role assignments must not be allowed (s6.4)
        builder.HasKey(x => new { x.UserId, x.RoleId });

        #endregion
    }
}
