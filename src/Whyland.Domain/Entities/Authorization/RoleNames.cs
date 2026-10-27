namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// Role names the application itself depends on, so the seeder and the guards that protect
/// those accounts never disagree on a re-typed string (s6.1.3).
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// The seeded account holding every permission. Deactivating it would lock the panel out,
    /// so the admin users page refuses to.
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";
}
