using Microsoft.Extensions.Logging;
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
    private const string SuperAdminRoleName = "SuperAdmin";

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
            new()
            {
                Name = SuperAdminRoleName,
                Title = "Super Administrator",
                IsSystem = true,
            },
            new()
            {
                Name = "Admin",
                Title = "Administrator",
                IsSystem = true,
            },
            new()
            {
                Name = "Instructor",
                Title = "Instructor",
                IsSystem = true,
            },
            new()
            {
                Name = "User",
                Title = "Regular User",
                IsSystem = true,
            },
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

            user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                PhoneNumber = phoneNumber,
                Email = string.IsNullOrWhiteSpace(admin.Email) ? null : admin.Email.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // A development default, hashed only while the account is being created: an existing
            // password is never overwritten by a restart.
            user.PasswordHash = new BCryptPasswordHasher().Hash(admin.Password);

            await context.Users.AddAsync(user);
            await context.UserRoles.AddAsync(
                new UserRole
                {
                    UserId = user.Id,
                    RoleId = superAdminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                }
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
                new UserRole
                {
                    UserId = user.Id,
                    RoleId = superAdminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                }
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
                user.PhoneNumber = phoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
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
            missingPermissions.Select(permission => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow,
            })
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
            new()
            {
                Name = "Course View",
                Key = Permissions.Courses.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Course Create",
                Key = Permissions.Courses.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Course Edit",
                Key = Permissions.Courses.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Course Delete",
                Key = Permissions.Courses.Delete,
                IsSystem = true,
            },
            // Category
            new()
            {
                Name = "Category View",
                Key = Permissions.Categories.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Category Create",
                Key = Permissions.Categories.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Category Edit",
                Key = Permissions.Categories.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Category Delete",
                Key = Permissions.Categories.Delete,
                IsSystem = true,
            },
            // User
            new()
            {
                Name = "User View",
                Key = Permissions.Users.View,
                IsSystem = true,
            },
            new()
            {
                Name = "User Create",
                Key = Permissions.Users.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "User Edit",
                Key = Permissions.Users.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "User Delete",
                Key = Permissions.Users.Delete,
                IsSystem = true,
            },
            // Role
            new()
            {
                Name = "Role View",
                Key = Permissions.Roles.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Role Create",
                Key = Permissions.Roles.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Role Edit",
                Key = Permissions.Roles.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Role Delete",
                Key = Permissions.Roles.Delete,
                IsSystem = true,
            },
            // Permission
            new()
            {
                Name = "Permission View",
                Key = Permissions.PermissionManagement.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Permission Create",
                Key = Permissions.PermissionManagement.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Permission Edit",
                Key = Permissions.PermissionManagement.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Permission Delete",
                Key = Permissions.PermissionManagement.Delete,
                IsSystem = true,
            },
            // Order
            new()
            {
                Name = "Order View",
                Key = Permissions.Orders.View,
                IsSystem = true,
            },
            // Payment
            new()
            {
                Name = "Payment View",
                Key = Permissions.Payments.View,
                IsSystem = true,
            },
            // Coupon
            new()
            {
                Name = "Coupon View",
                Key = Permissions.Coupons.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Coupon Create",
                Key = Permissions.Coupons.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Coupon Edit",
                Key = Permissions.Coupons.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Coupon Delete",
                Key = Permissions.Coupons.Delete,
                IsSystem = true,
            },
            // Blog
            new()
            {
                Name = "Blog View",
                Key = Permissions.Blogs.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Blog Create",
                Key = Permissions.Blogs.Create,
                IsSystem = true,
            },
            new()
            {
                Name = "Blog Edit",
                Key = Permissions.Blogs.Edit,
                IsSystem = true,
            },
            new()
            {
                Name = "Blog Delete",
                Key = Permissions.Blogs.Delete,
                IsSystem = true,
            },
            // Setting
            new()
            {
                Name = "Setting View",
                Key = Permissions.Settings.View,
                IsSystem = true,
            },
            new()
            {
                Name = "Setting Edit",
                Key = Permissions.Settings.Edit,
                IsSystem = true,
            },
        ];
}
