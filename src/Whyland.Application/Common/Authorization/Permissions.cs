namespace Whyland.Application.Common.Authorization;

/// <summary>
///     Contains all system permission keys used throughout the application
///     for authorization.
/// </summary>
/// <remarks>
///     These keys are used with the <see cref="HasPermissionAttribute"/>
///     to secure controllers and actions. Each key follows the pattern:
///     "{resource}.{action}" (e.g., "course.view").
///     <para>
///         This class is the single source of truth for permission keys.
///         Always reference these constants instead of using raw strings,
///         so typos are caught at compile time.
///     </para>
/// </remarks>
public static class Permissions
{
    /// <summary>Course management permissions.</summary>
    public static class Courses
    {
        /// <summary>Permission to view courses.</summary>
        public const string View = "course.view";
        /// <summary>Permission to create courses.</summary>
        public const string Create = "course.create";
        /// <summary>Permission to edit existing courses.</summary>
        public const string Edit = "course.edit";
        /// <summary>Permission to delete courses.</summary>
        public const string Delete = "course.delete";
    }

    /// <summary>Category management permissions.</summary>
    public static class Categories
    {
        /// <summary>Permission to view categories.</summary>
        public const string View = "category.view";
        /// <summary>Permission to create categories.</summary>
        public const string Create = "category.create";
        /// <summary>Permission to edit existing categories.</summary>
        public const string Edit = "category.edit";
        /// <summary>Permission to delete categories.</summary>
        public const string Delete = "category.delete";
    }

    /// <summary>User management permissions.</summary>
    public static class Users
    {
        /// <summary>Permission to view users.</summary>
        public const string View = "user.view";
        /// <summary>Permission to create users.</summary>
        public const string Create = "user.create";
        /// <summary>Permission to edit existing users.</summary>
        public const string Edit = "user.edit";
        /// <summary>Permission to delete users.</summary>
        public const string Delete = "user.delete";
    }

    /// <summary>Role management permissions.</summary>
    public static class Roles
    {
        /// <summary>Permission to view roles.</summary>
        public const string View = "role.view";
        /// <summary>Permission to create roles.</summary>
        public const string Create = "role.create";
        /// <summary>Permission to edit existing roles.</summary>
        public const string Edit = "role.edit";
        /// <summary>Permission to delete roles.</summary>
        public const string Delete = "role.delete";
    }

    /// <summary>Permission management permissions.</summary>
    public static class PermissionManagement
    {
        /// <summary>Permission to view assigned permissions.</summary>
        public const string View = "permission.view";
        /// <summary>Permission to grant new permissions.</summary>
        public const string Create = "permission.create";
        /// <summary>Permission to change existing permissions.</summary>
        public const string Edit = "permission.edit";
        /// <summary>Permission to revoke permissions.</summary>
        public const string Delete = "permission.delete";
    }

    /// <summary>Order permissions.</summary>
    public static class Orders
    {
        /// <summary>Permission to view orders.</summary>
        public const string View = "order.view";
    }

    /// <summary>Payment permissions.</summary>
    public static class Payments
    {
        /// <summary>Permission to view payments.</summary>
        public const string View = "payment.view";
    }

    /// <summary>Coupon management permissions.</summary>
    public static class Coupons
    {
        /// <summary>Permission to view coupons.</summary>
        public const string View = "coupon.view";
        /// <summary>Permission to create coupons.</summary>
        public const string Create = "coupon.create";
        /// <summary>Permission to edit existing coupons.</summary>
        public const string Edit = "coupon.edit";
        /// <summary>Permission to delete coupons.</summary>
        public const string Delete = "coupon.delete";
    }

    /// <summary>Blog management permissions.</summary>
    public static class Blogs
    {
        /// <summary>Permission to view blog posts.</summary>
        public const string View = "blog.view";
        /// <summary>Permission to create blog posts.</summary>
        public const string Create = "blog.create";
        /// <summary>Permission to edit existing blog posts.</summary>
        public const string Edit = "blog.edit";
        /// <summary>Permission to delete blog posts.</summary>
        public const string Delete = "blog.delete";
    }

    /// <summary>System settings permissions.</summary>
    public static class Settings
    {
        /// <summary>Permission to view system settings.</summary>
        public const string View = "setting.view";
        /// <summary>Permission to edit system settings.</summary>
        public const string Edit = "setting.edit";
    }

    /// <summary>Dashboard access permissions.</summary>
    public static class Dashboard
    {
        /// <summary>Permission to view the admin dashboard.</summary>
        public const string View = "dashboard.view";
    }
}
