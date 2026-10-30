namespace Whyland.Application.Features.Admin.Users.Queries.GetDetails;

/// <summary>
/// Reads everything the admin details modal shows about one account.
/// </summary>
/// <param name="Id">The account to read.</param>
public sealed record GetUserDetailsQuery(Guid Id) : IQuery<UserDetailsDto>;
