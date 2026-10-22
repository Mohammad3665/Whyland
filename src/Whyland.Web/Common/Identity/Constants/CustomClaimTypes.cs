namespace Whyland.Web.Common.Identity.Constants;

/// <summary>
///     Defines custom claim types used throughout the application
///     for authentication and authorization purposes.
/// </summary>
/// <remarks>
///     Using constants instead of raw strings prevents typos and
///     keeps claim types consistent between token/claim generation
///     and authorization handlers.
///     <para>
///         Example usage:
///         <code>
///             new Claim(CustomClaimTypes.Permissions, "course.create")
///         </code>
///     </para>
/// </remarks>
public static class CustomClaimTypes
{
    /// <summary>
    ///     Claim type that holds a single permission key granted to the user.
    /// </summary>
    /// <remarks>
    ///     A user may have multiple claims of this type — one per permission.
    ///     The value must match one of the keys defined in
    ///     <see cref="Permissions"/> (e.g., "course.view", "user.create").
    ///     <para>
    ///         This claim is read by
    ///         <c>PermissionAuthorizationHandler</c> to authorize requests
    ///         decorated with <c>[HasPermission(...)]</c>.
    ///     </para>
    /// </remarks>
    public const string Permissions = "permissions";
}