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
    private const string AdminRoleName = RoleNames.Admin;
    private const string InstructorRoleName = RoleNames.Instructor;

    /// <summary>
    ///     The permissions each seeded staff role holds.
    ///     <para>
    ///     SuperAdmin is absent on purpose — it is granted every key there is, including the ones a
    ///     later module adds. The ordinary user is absent for the opposite reason: the public site is
    ///     not an admin panel, and nothing a customer does is guarded by a permission key.
    ///     </para>
    ///     <para>
    ///     Every entry names the constant a page checks instead of re-typing the key text, so renaming
    ///     a key in <see cref="Permissions"/> is a compile error here rather than a role that quietly
    ///     stops being able to do its job.
    ///     </para>
    /// </summary>
    private static readonly Dictionary<string, string[]> RolePermissionBaseline = new(StringComparer.Ordinal)
    {
        // Runs the panel end to end, except the two modules that hand out authority: a manager who
        // could grant themselves the keys above them would leave no seat that outranks them.
        [AdminRoleName] =
        [
            Permissions.Dashboard.View,
            Permissions.Courses.View, Permissions.Courses.Create, Permissions.Courses.Edit, Permissions.Courses.Delete,
            Permissions.Categories.View, Permissions.Categories.Create, Permissions.Categories.Edit, Permissions.Categories.Delete,
            Permissions.Users.View, Permissions.Users.Create, Permissions.Users.Edit, Permissions.Users.Delete,
            Permissions.Orders.View,
            Permissions.Payments.View,
            Permissions.Coupons.View, Permissions.Coupons.Create, Permissions.Coupons.Edit, Permissions.Coupons.Delete,
            Permissions.Blogs.View, Permissions.Blogs.Create, Permissions.Blogs.Edit, Permissions.Blogs.Delete,
            Permissions.BlogCategories.View, Permissions.BlogCategories.Create, Permissions.BlogCategories.Edit, Permissions.BlogCategories.Delete,
            Permissions.Faqs.View, Permissions.Faqs.Create, Permissions.Faqs.Edit, Permissions.Faqs.Delete,
            Permissions.Settings.View, Permissions.Settings.Edit,
        ],

        // Teaches: writes courses and reads the catalogue a course hangs on. Deleting a published
        // course, touching another person's account, or the site's own content and settings is not
        // part of the job, so those keys stay with Admin and SuperAdmin.
        [InstructorRoleName] =
        [
            Permissions.Dashboard.View,
            Permissions.Courses.View, Permissions.Courses.Create, Permissions.Courses.Edit,
            Permissions.Categories.View,
        ],
    };

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
        await BackfillDefaultRoleAsync(context);
    }

    /// <summary>
    /// Gives the ordinary-user role to every live account that does not hold it (s6.4). The creation
    /// paths enforce the same floor in the repository; this repairs the accounts that predate it, so
    /// the rule "every user has the User role" holds for rows already in the database too.
    /// </summary>
    private static async Task BackfillDefaultRoleAsync(WhylandDbContext context)
    {
        var defaultRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.User);
        if (defaultRole is null)
            return;

        var missing = await context.Users
            .Where(u => !context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == defaultRole.Id))
            .Select(u => u.Id)
            .ToListAsync();

        if (missing.Count == 0)
            return;

        foreach (var userId in missing)
        {
            await context.UserRoles.AddAsync(UserRole.Create(userId, defaultRole.Id));
        }

        await context.SaveChangesAsync();
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
            Created(Role.Create(AdminRoleName, "مدیر", true)),
            Created(Role.Create(InstructorRoleName, "مدرس", true)),
            Created(Role.Create(RoleNames.User, "کاربر عادی", true)),
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the system permissions (key list from s6.2). The insert is keyed on the permission key
    /// rather than on an empty table, so a database created before a module existed still receives the
    /// keys added later — otherwise a new [HasPermission] would deny everyone, including the admin.
    /// </summary>
    private static async Task SeedPermissionsAsync(WhylandDbContext context)
    {
        var wanted = GetPermissionsList();

        var existingKeys = await context.Permissions.IgnoreQueryFilters()
            .Select(x => x.Key)
            .ToListAsync();

        var missing = wanted.Where(x => !existingKeys.Contains(x.Key)).ToList();

        if (missing.Count == 0)
            return;

        await context.Permissions.AddRangeAsync(missing);
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

        var user = adminUserId == default
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
    /// Grants the seeded roles their permissions (permission-role assignment, s6.3).
    /// <para>
    /// Both rules are declarative, and they have to be: every role seeded here is a system role, whose
    /// grants the panel shows but refuses to edit, so this file — not an operator — is what decides
    /// them. Editing <see cref="RolePermissionBaseline"/> and restarting is the supported way to change
    /// a staff role; a key a later module adds reaches its role at the next start on its own.
    /// </para>
    /// <para>
    /// Only grants are added, never removed, so a permission taken away in the database by hand is not
    /// silently handed back every restart beyond what the baseline still asks for.
    /// </para>
    /// </summary>
    private static async Task SeedRolePermissionsAsync(WhylandDbContext context)
    {
        var roles = await context.Roles
            .Where(x => x.Name == SuperAdminRoleName
                     || x.Name == AdminRoleName
                     || x.Name == InstructorRoleName)
            .ToListAsync();

        if (roles.Count == 0)
            return;

        // The baseline is written in key text, not in identifiers: these rows are created by the same
        // run, so their ids are only knowable here, and a re-typed id could not be checked at compile
        // time against what the pages ask for.
        var permissions = await context.Permissions.ToListAsync();
        var idByKey = permissions.ToDictionary(x => x.Key, x => x.Id, StringComparer.Ordinal);

        var existing = (await context.RolePermissions
                .Select(x => new { x.RoleId, x.PermissionId })
                .ToListAsync())
            .Select(x => (x.RoleId, x.PermissionId))
            .ToHashSet();

        var wanted = new List<(RoleId Role, PermissionId Permission)>();

        foreach (var role in roles)
        {
            // The account that can repair the panel holds every key there is.
            if (role.Name == SuperAdminRoleName)
            {
                wanted.AddRange(permissions.Select(x => (role.Id, x.Id)));
                continue;
            }

            if (!RolePermissionBaseline.TryGetValue(role.Name, out var keys))
                continue;

            foreach (var key in keys)
            {
                // A baseline naming a key the permission list does not seed is a mistake in this file,
                // so it stops the startup instead of handing the role a silent nothing.
                if (!idByKey.TryGetValue(key, out var permissionId))
                    throw new InvalidOperationException(
                        $"The {role.Name} baseline grants {key}, which the permission list does not seed.");

                wanted.Add((role.Id, permissionId));
            }
        }

        var missing = wanted.Where(x => !existing.Contains(x)).ToList();

        if (missing.Count == 0)
            return;

        await context.RolePermissions.AddRangeAsync(
            missing.Select(x => RolePermission.Create(x.Role, x.Permission))
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
            Created(Permission.Create("مشاهده دوره‌ها", Permissions.Courses.View)),
            Created(Permission.Create("ایجاد دوره‌ها", Permissions.Courses.Create)),
            Created(Permission.Create("ویرایش دوره‌ها", Permissions.Courses.Edit)),
            Created(Permission.Create("حذف دوره‌ها", Permissions.Courses.Delete)),
            // Category
            Created(Permission.Create("مشاهده دسته‌بندی‌ها", Permissions.Categories.View)),
            Created(Permission.Create("ایجاد دسته‌بندی‌ها", Permissions.Categories.Create)),
            Created(Permission.Create("ویرایش دسته‌بندی‌ها", Permissions.Categories.Edit)),
            Created(Permission.Create("حذف دسته‌بندی‌ها", Permissions.Categories.Delete)),
            // User
            Created(Permission.Create("مشاهده کاربران", Permissions.Users.View)),
            Created(Permission.Create("ایجاد کاربران", Permissions.Users.Create)),
            Created(Permission.Create("ویرایش کاربران", Permissions.Users.Edit)),
            Created(Permission.Create("حذف کاربران", Permissions.Users.Delete)),
            // Role
            Created(Permission.Create("مشاهده نقش‌ها", Permissions.Roles.View)),
            Created(Permission.Create("ایجاد نقش‌ها", Permissions.Roles.Create)),
            Created(Permission.Create("ویرایش نقش‌ها", Permissions.Roles.Edit)),
            Created(Permission.Create("حذف نقش‌ها", Permissions.Roles.Delete)),
            // Permission
            Created(Permission.Create("مشاهده مجوزها", Permissions.PermissionManagement.View)),
            Created(Permission.Create("ایجاد مجوزها", Permissions.PermissionManagement.Create)),
            Created(Permission.Create("ویرایش مجوزها", Permissions.PermissionManagement.Edit)),
            Created(Permission.Create("حذف مجوزها", Permissions.PermissionManagement.Delete)),
            // Order
            Created(Permission.Create("مشاهده سفارش‌ها", Permissions.Orders.View)),
            // Payment
            Created(Permission.Create("مشاهده پرداخت‌ها", Permissions.Payments.View)),
            // Coupon
            Created(Permission.Create("مشاهده کدهای تخفیف", Permissions.Coupons.View)),
            Created(Permission.Create("ایجاد کدهای تخفیف", Permissions.Coupons.Create)),
            Created(Permission.Create("ویرایش کدهای تخفیف", Permissions.Coupons.Edit)),
            Created(Permission.Create("حذف کدهای تخفیف", Permissions.Coupons.Delete)),
            // Blog
            Created(Permission.Create("مشاهده وبلاگ‌ها", Permissions.Blogs.View)),
            Created(Permission.Create("ایجاد وبلاگ‌ها", Permissions.Blogs.Create)),
            Created(Permission.Create("ویرایش وبلاگ‌ها", Permissions.Blogs.Edit)),
            Created(Permission.Create("حذف وبلاگ‌ها", Permissions.Blogs.Delete)),
            // Blog category (the taxonomy, not the posts)
            Created(Permission.Create("مشاهده دسته‌بندی‌های مقالات", Permissions.BlogCategories.View)),
            Created(Permission.Create("ایجاد دسته‌بندی‌های مقالات", Permissions.BlogCategories.Create)),
            Created(Permission.Create("ویرایش دسته‌بندی‌های مقالات", Permissions.BlogCategories.Edit)),
            Created(Permission.Create("حذف دسته‌بندی‌های مقالات", Permissions.BlogCategories.Delete)),
            // Faq (site FAQs, not the course-scoped ones)
            Created(Permission.Create("مشاهده سوالات متداول", Permissions.Faqs.View)),
            Created(Permission.Create("ایجاد سوالات متداول", Permissions.Faqs.Create)),
            Created(Permission.Create("ویرایش سوالات متداول", Permissions.Faqs.Edit)),
            Created(Permission.Create("حذف سوالات متداول", Permissions.Faqs.Delete)),
            // Setting
            Created(Permission.Create("مشاهده تنظیمات", Permissions.Settings.View)),
            Created(Permission.Create("ویرایش تنظیمات", Permissions.Settings.Edit)),
            // Dashboard: the AdminPanelAccess policy of the whole admin area is pinned to this key,
            // so without it no account — not even the super admin — can open a single admin page.
            Created(Permission.Create("مشاهده داشبورد", Permissions.Dashboard.View)),
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
