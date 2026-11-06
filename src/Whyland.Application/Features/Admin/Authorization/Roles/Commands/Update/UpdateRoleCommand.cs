namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Update;

/// <summary>
/// Renames an existing custom role and gives it a new display title.
/// <para>
/// System roles are excluded from this command's whole path: their names are the strings the seeders
/// and the authorization code refer to, so renaming one would silently break the assignment it exists
/// for. The handler reports that rather than trusting the panel to hide the button.
/// </para>
/// </summary>
/// <param name="Id">The identifier of the role to update.</param>
/// <param name="Name">The new unique role name.</param>
/// <param name="Title">The new display title.</param>
public sealed record UpdateRoleCommand(
    long Id,
    string Name,
    string Title) : ICommand;
