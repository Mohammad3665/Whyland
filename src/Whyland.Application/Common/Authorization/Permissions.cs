using System;

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
        public const string View = "course.view";
        public const string Create = "course.create";
        public const string Edit = "course.edit";
        public const string Delete = "course.delete";
    }

    /// <summary>Category management permissions.</summary>
    public static class Categories
    {
        public const string View = "category.view";
        public const string Create = "category.create";
        public const string Edit = "category.edit";
        public const string Delete = "category.delete";
    }

    /// <summary>User management permissions.</summary>
    public static class Users
    {
        public const string View = "user.view";
        public const string Create = "user.create";
        public const string Edit = "user.edit";
        public const string Delete = "user.delete";
    }

    /// <summary>Role management permissions.</summary>
    public static class Roles
    {
        public const string View = "role.view";
        public const string Create = "role.create";
        public const string Edit = "role.edit";
        public const string Delete = "role.delete";
    }

    /// <summary>Permission management permissions.</summary>
    public static class PermissionManagement
    {
        public const string View = "permission.view";
        public const string Create = "permission.create";
        public const string Edit = "permission.edit";
        public const string Delete = "permission.delete";
    }

    /// <summary>Order permissions.</summary>
    public static class Orders
    {
        public const string View = "order.view";
    }

    /// <summary>Payment permissions.</summary>
    public static class Payments
    {
        public const string View = "payment.view";
    }

    /// <summary>Coupon management permissions.</summary>
    public static class Coupons
    {
        public const string View = "coupon.view";
        public const string Create = "coupon.create";
        public const string Edit = "coupon.edit";
        public const string Delete = "coupon.delete";
    }

    /// <summary>Blog management permissions.</summary>
    public static class Blogs
    {
        public const string View = "blog.view";
        public const string Create = "blog.create";
        public const string Edit = "blog.edit";
        public const string Delete = "blog.delete";
    }

    /// <summary>System settings permissions.</summary>
    public static class Settings
    {
        public const string View = "setting.view";
        public const string Edit = "setting.edit";
    }

    /// <summary>Dashboard access permissions.</summary>
    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }
}
