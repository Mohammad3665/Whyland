namespace Whyland.Infrastructure.Persistence.Context;

/// <summary>
/// Constants for the logical schema groups used to organize tables in PostgreSQL.
/// Each bounded area of the system owns exactly one schema (s3.8 of the SRS).
/// </summary>
public static class SchemaConstants
{
    /// <summary>Authentication, profiles, roles and permissions.</summary>
    public const string Identity = "identity";

    /// <summary>Courses, episodes and other teaching content.</summary>
    public const string Catalog = "catalog";

    /// <summary>Orders, payments, invoices and coupons.</summary>
    public const string Commerce = "commerce";

    /// <summary>Purchase records and per-episode viewing progress.</summary>
    public const string Learning = "learning";

    /// <summary>Blog posts and blog categories.</summary>
    public const string Blog = "blog";

    /// <summary>User-facing engagement records such as favorites.</summary>
    public const string Interactions = "interactions";

    /// <summary>Editable site configuration entries.</summary>
    public const string Settings = "settings";

    /// <summary>General purpose content such as FAQs.</summary>
    public const string Content = "content";
}
