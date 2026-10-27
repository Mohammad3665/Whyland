namespace Whyland.Application.Features.Admin.Users.Commands.Update;

/// <summary>
/// Updates an existing account from the admin users modal.
/// <para>
/// The password is deliberately absent: changing credentials is its own flow
/// (<c>ChangePassword</c> for the owner, <c>SetPassword</c> for a first password), so an admin
/// editing a profile can never overwrite a secret silently.
/// </para>
/// </summary>
/// <param name="Id">The account to update.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="Email">Optional e-mail; unique when present.</param>
/// <param name="PhoneNumber">Optional phone number; the sign-in and recovery identifier.</param>
/// <param name="IsActive">Whether the account may authenticate.</param>
/// <param name="RoleIds">The full set of roles the account should hold afterwards.</param>
public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    List<long> RoleIds) : ICommand;
