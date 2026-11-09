namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Create;

/// <summary>
/// Creates a custom role from the admin roles modal and returns its identifier.
/// <para>
/// A role created here is never a system one: <c>IsSystem</c> is set only by the seeder, because it
/// marks the roles the code itself looks up by name. Letting the panel raise the flag would give an
/// operator a way to make a row the application cannot delete.
/// </para>
/// </summary>
/// <param name="Name">Unique role name, e.g. "ContentEditor".</param>
/// <param name="Title">Display title shown to operators, e.g. "ویرایشگر محتوا".</param>
public sealed record CreateRoleCommand(
    string Name,
    string Title) : ICommand<RoleId>;
