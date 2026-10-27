namespace Whyland.Shared.Dtos.Identity.User;

/// <summary>
/// An account as a picker needs it: the identifier that is submitted and the full name that
/// is displayed. Used where a filter or a form refers to a user by id, such as the author of
/// a row (<c>CreatedBy</c> / <c>UpdatedAt</c>) — a raw guid is never shown to the admin.
/// </summary>
/// <param name="Id">The account identifier.</param>
/// <param name="Name">The display name, first and last name together.</param>
public sealed record UserOptionDto(Guid Id, string Name);
