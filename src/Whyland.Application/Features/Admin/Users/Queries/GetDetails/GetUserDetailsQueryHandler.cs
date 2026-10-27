using Microsoft.Extensions.Logging;
using Whyland.Application.Features.Admin.Users.Errors;
using Whyland.Shared.Dtos.Identity.User;

namespace Whyland.Application.Features.Admin.Users.Queries.GetDetails;

/// <summary>
/// Handles <see cref="GetUserDetailsQuery"/> by reading the account, its roles, its profile
/// and its external provider links.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to user query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetUserDetailsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetUserDetailsQueryHandler> logger) : IQueryHandler<GetUserDetailsQuery, UserDetailsDto>
{
    /// <summary>
    /// Returns the details, a not-found error when the account does not exist, or a masked
    /// error when the read fails.
    /// </summary>
    /// <param name="query">The query naming the account.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<Result<UserDetailsDto>> Handle(GetUserDetailsQuery query, CancellationToken ct)
    {
        try
        {
            var details = await unitOfWork.UserQueryRepository.GetDetailsAsync(query.Id, ct);
            return details is null
                ? Result<UserDetailsDto>.Failure(UserAdminErrors.NotFound(query.Id))
                : Result<UserDetailsDto>.Success(details);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading user details failed for {UserId}.", query.Id);
            return Result<UserDetailsDto>.Failure(UserAdminErrors.ListFailed());
        }
    }
}
