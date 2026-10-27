namespace Whyland.Application.Features.Auth.Password.RequestPhoneBindingCode;

/// <summary>
/// Asks for an SMS code that proves the signed-in user owns the phone number they want to attach
/// to the account (s5.4). The number is collected when a password is being set, because it is what
/// a forgotten password will later be reset through.
/// </summary>
/// <param name="UserId">The signed-in user, taken from the cookie claims by the web layer.</param>
/// <param name="PhoneNumber">The number to bind, in any Persian/international spelling.</param>
public sealed record RequestPhoneBindingCodeCommand(
    Guid UserId,
    string PhoneNumber
) : ICommand;
