namespace Whyland.Application.Features.Auth.Queries.PasswordState;

/// <summary>
/// Handles <see cref="GetPasswordPageStateQuery"/> by reading the stored account row.
/// </summary>
/// <param name="unitOfWork">Provides the user read.</param>
public sealed class GetPasswordPageStateQueryHandler(
    IUnitOfWork unitOfWork
) : IQueryHandler<GetPasswordPageStateQuery, PasswordPageStateDto>
{
    /// <summary>Returns the two flags the pages render on.</summary>
    public async Task<Result<PasswordPageStateDto>> Handle(GetPasswordPageStateQuery query, CancellationToken ct)
    {
        var user = await unitOfWork.UserQueryRepository.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return UserErrors.InvalidCredentials();

        return new PasswordPageStateDto(
            RequiresPhoneNumber: string.IsNullOrWhiteSpace(user.PhoneNumber),
            HasPassword: !string.IsNullOrWhiteSpace(user.PasswordHash));
    }
}
