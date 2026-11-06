using Role = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Domain.Model.Authorization.Common.Constants;

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

    /// <summary>
    /// The seeded managerial role: it runs the panel — catalogue, content, accounts and commerce —
    /// but does not hand out authority, which stays with <see cref="SuperAdmin"/>.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// The seeded teaching role: it writes the courses and reads the catalogue they hang on.
    /// </summary>
    public const string Instructor = "Instructor";

    /// <summary>
    /// The role every account gets whether it registered itself, came through a provider, or was
    /// typed in by an operator: a user with no role at all cannot buy or follow anything, so this is
    /// a floor the creation paths enforce rather than a choice the admin panel makes.
    /// </summary>
    public const string User = "User";
}
