namespace Whyland.Application.Features.Admin.Users.Commands.ToggleStatus;

/// <summary>
/// Flips an account between active and inactive and returns the new state
/// (s5.1.3: an inactive user must not authenticate).
/// </summary>
/// <param name="Id">The account to change.</param>
public sealed record ToggleUserStatusCommand(Guid Id) : ICommand<bool>;
