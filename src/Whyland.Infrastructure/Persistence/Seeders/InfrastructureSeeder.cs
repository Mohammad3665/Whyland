using Whyland.Application.Common.Authorization;
using Whyland.Application.Common.Options;
using Whyland.Application.Common.Validation;
using Whyland.Infrastructure.Services;

namespace Whyland.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds initial system data required for first operation (s6.6):
/// system roles, system permissions, permission-role assignments and a super admin user.
/// Seeded roles/permissions remain dynamic database records; authorization must never
/// rely on hard-coded role names (s6.6).
/// Seed values are development defaults and must be rotated in real deployments.
/// </summary>
public static class InfrastructureSeeder
{
    private const string SuperAdminRoleName = RoleNames.SuperAdmin;

    /// <summary>
    /// Seeds the database with default roles, permissions, and a super admin user.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="admin">The account the seeder creates or keeps, read from configuration.</param>
    /// <param name="logger">Reports the cases where the configured admin cannot be used.</param>
    public static async Task SeedDatabaseAsync(
        this WhylandDbContext context,
        AdminAccountOptions admin,
        ILogger logger)
    {
        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedSuperAdminAsync(context, admin, logger);
        await SeedRolePermissionsAsync(context);
    }

    /// <summary>
    /// Seeds the default system roles if none exist (s6.6).
    /// </summary>
    private static async Task SeedRolesAsync(WhylandDbContext context)
    {
        if (await context.Roles.IgnoreQueryFilters().AnyAsync())
            return;

        var roles = new List<Role>
        {
            Created(Role.Create(SuperAdminRoleName, "مدیر ارشد", true)),
            Created(Role.Create("Admin", "مدیر", true)),
            Created(Role.Create("Instructor", "مدرس", true)),
            Created(Role.Create("User", "کاربر عادی", true)),
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the system permissions if none exist (key list from s6.2).
    /// </summary>
    private static async Task SeedPermissionsAsync(WhylandDbContext context)
    {
        if (await context.Permissions.IgnoreQueryFilters().AnyAsync())
            return;

        await context.Permissions.AddRangeAsync(GetPermissionsList());
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates or keeps the super admin account. It is recognised by the role it holds rather than by
    /// any hard-coded handle, because the account has no username any more and password sign-in
    /// resolves it by phone number alone (s5.1.1). Moving the admin to a new number is therefore a
    /// settings change, not a code change.
    /// </summary>
    private static async Task SeedSuperAdminAsync(
        WhylandDbContext context,
        AdminAccountOptions admin,
        ILogger logger)
    {
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(x =>
            x.Name == SuperAdminRoleName
        );

        if (superAdminRole is null)
            return;

        // Normalized exactly like the login flow normalizes what the admin types, or the two values
        // would never match.
        var phoneNumber = string.IsNullOrWhiteSpace(admin.PhoneNumber)
            ? null
            : PhoneNormalizer.Normalize(admin.PhoneNumber);

        var adminUserId = await context.UserRoles
            .Where(x => x.RoleId == superAdminRole.Id)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync();

        var user = adminUserId == Guid.Empty
            ? null
            : await context.Users.FirstOrDefaultAsync(x => x.Id == adminUserId);

        if (user is null)
        {
            if (phoneNumber is null)
            {
                logger.LogWarning(
                    "Admin:PhoneNumber is not configured, so no super admin was created. Password " +
                    "sign-in needs a phone number; see Documentation/Whyland-Auth-Setup.md.");
                return;
            }

            if (await context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber))
            {
                logger.LogWarning(
                    "Admin:PhoneNumber {PhoneNumber} already belongs to another account, so no super " +
                    "admin was created.", phoneNumber);
                return;
            }

            var created = User.Create(
                admin.FirstName,
                admin.LastName,
                phoneNumber: phoneNumber,
                email: string.IsNullOrWhiteSpace(admin.Email) ? null : admin.Email.Trim());

            // Both names come from settings, so a pair the account rules refuse is a configuration
            // mistake to report — not a row to force into the table.
            if (created.IsFailure)
            {
                logger.LogWarning(
                    "Admin:FirstName/LastName were refused by the account rules ({Code}), so no super " +
                    "admin was created.", created.Error?.Code);
                return;
            }

            user = created.Data!;

            // A development default, hashed only while the account is being created: an existing
            // password is never overwritten by a restart.
            user.SetPassword(new BCryptPasswordHasher().Hash(admin.Password));

            await context.Users.AddAsync(user);

            // The audit columns are not set here: the interceptor stamps CreatedAt/UpdatedAt for
            // every IAuditable entry on save, and UserRole takes its own timestamp on creation.
            await context.UserRoles.AddAsync(
                UserRole.Create(user.Id, superAdminRole.Id)
            );
            await context.SaveChangesAsync();
            return;
        }

        // Ensure the existing super admin holds the super admin role, without re-inserting an
        // assignment that would violate the composite key.
        var hasAssignment = await context.UserRoles.AnyAsync(x =>
            x.UserId == user.Id && x.RoleId == superAdminRole.Id
        );

        if (!hasAssignment)
        {
            await context.UserRoles.AddAsync(
                UserRole.Create(user.Id, superAdminRole.Id)
            );
        }

        if (phoneNumber is not null && user.PhoneNumber != phoneNumber)
        {
            if (await context.Users.AnyAsync(x => x.Id != user.Id && x.PhoneNumber == phoneNumber))
            {
                logger.LogWarning(
                    "Admin:PhoneNumber {PhoneNumber} belongs to another account, so the super admin " +
                    "keeps its current number.", phoneNumber);
            }
            else
            {
                user.BindPhoneNumber(phoneNumber);
                logger.LogInformation("Super admin sign-in number set to {PhoneNumber}.", phoneNumber);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Grants every seeded permission to the SuperAdmin role (permission-role assignment, s6.3).
    /// </summary>
    private static async Task SeedRolePermissionsAsync(WhylandDbContext context)
    {
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(x =>
            x.Name == SuperAdminRoleName
        );

        if (superAdminRole is null)
            return;

        var existingRolePermissionIds = await context
            .RolePermissions.Where(x => x.RoleId == superAdminRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync();

        var missingPermissions = await context
            .Permissions.Where(x => !existingRolePermissionIds.Contains(x.Id))
            .ToListAsync();

        if (missingPermissions.Count == 0)
            return;

        await context.RolePermissions.AddRangeAsync(
            missingPermissions.Select(permission =>
                RolePermission.Create(superAdminRole.Id, permission.Id)
            )
        );

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     The initial system permission list from s6.2 of the SRS.
    ///     The list will evolve together with the admin panel modules.
    /// </summary>
    private static List<Permission> GetPermissionsList() =>
        [
            // Course
            Created(Permission.Create("Course View", Permissions.Courses.View)),
            Created(Permission.Create("Course Create", Permissions.Courses.Create)),
            Created(Permission.Create("Course Edit", Permissions.Courses.Edit)),
            Created(Permission.Create("Course Delete", Permissions.Courses.Delete)),
            // Category
            Created(Permission.Create("Category View", Permissions.Categories.View)),
            Created(Permission.Create("Category Create", Permissions.Categories.Create)),
            Created(Permission.Create("Category Edit", Permissions.Categories.Edit)),
            Created(Permission.Create("Category Delete", Permissions.Categories.Delete)),
            // User
            Created(Permission.Create("User View", Permissions.Users.View)),
            Created(Permission.Create("User Create", Permissions.Users.Create)),
            Created(Permission.Create("User Edit", Permissions.Users.Edit)),
            Created(Permission.Create("User Delete", Permissions.Users.Delete)),
            // Role
            Created(Permission.Create("Role View", Permissions.Roles.View)),
            Created(Permission.Create("Role Create", Permissions.Roles.Create)),
            Created(Permission.Create("Role Edit", Permissions.Roles.Edit)),
            Created(Permission.Create("Role Delete", Permissions.Roles.Delete)),
            // Permission
            Created(Permission.Create("Permission View", Permissions.PermissionManagement.View)),
            Created(Permission.Create("Permission Create", Permissions.PermissionManagement.Create)),
            Created(Permission.Create("Permission Edit", Permissions.PermissionManagement.Edit)),
            Created(Permission.Create("Permission Delete", Permissions.PermissionManagement.Delete)),
            // Order
            Created(Permission.Create("Order View", Permissions.Orders.View)),
            // Payment
            Created(Permission.Create("Payment View", Permissions.Payments.View)),
            // Coupon
            Created(Permission.Create("Coupon View", Permissions.Coupons.View)),
            Created(Permission.Create("Coupon Create", Permissions.Coupons.Create)),
            Created(Permission.Create("Coupon Edit", Permissions.Coupons.Edit)),
            Created(Permission.Create("Coupon Delete", Permissions.Coupons.Delete)),
            // Blog
            Created(Permission.Create("Blog View", Permissions.Blogs.View)),
            Created(Permission.Create("Blog Create", Permissions.Blogs.Create)),
            Created(Permission.Create("Blog Edit", Permissions.Blogs.Edit)),
            Created(Permission.Create("Blog Delete", Permissions.Blogs.Delete)),
            // Setting
            Created(Permission.Create("Setting View", Permissions.Settings.View)),
            Created(Permission.Create("Setting Edit", Permissions.Settings.Edit)),
        ];

    /// <summary>
    /// Reads the entity out of a factory result. A seeded definition that breaks the invariants of its
    /// own aggregate is a mistake in this file rather than user input, so the seeder stops the startup
    /// with the rejected code instead of inserting a row the domain refuses to build.
    /// </summary>
    /// <typeparam name="TEntity">The entity the factory was building.</typeparam>
    /// <param name="result">The result of the entity factory.</param>
    private static TEntity Created<TEntity>(Result<TEntity> result) =>
        result.IsFailure
            ? throw new InvalidOperationException(
                $"The seeded {typeof(TEntity).Name} is refused by its own invariants ({result.Error?.Code}).")
            : result.Data!;
}
