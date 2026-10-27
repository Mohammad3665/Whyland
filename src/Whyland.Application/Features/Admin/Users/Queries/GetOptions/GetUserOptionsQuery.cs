using Whyland.Shared.Dtos.Identity.User;

namespace Whyland.Application.Features.Admin.Users.Queries.GetOptions;

/// <summary>
/// Reads the accounts that are allowed to be listed in an author picker, so the admin picks a
/// person from the ones who could actually have done the action instead of the whole user table.
/// </summary>
/// <param name="Field">
/// The filter field being filled: <c>CreatedBy</c>, <c>UpdatedBy</c> or <c>DeletedBy</c>. The
/// permission it requires is decided here, never by the client.
/// </param>
public sealed record GetUserOptionsQuery(string Field) : IQuery<IReadOnlyList<UserOptionDto>>;
