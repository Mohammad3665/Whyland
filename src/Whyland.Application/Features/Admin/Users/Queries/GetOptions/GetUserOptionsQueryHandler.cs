using Whyland.Application.Common.Authorization;

namespace Whyland.Application.Features.Admin.Users.Queries.GetOptions;

/// <summary>
/// Handles <see cref="GetUserOptionsQuery"/> by reading the accounts that hold the permission
/// behind the requested author field. The mapping lives here so a client can only ever ask for
/// one of the three known fields and cannot enumerate the holders of an arbitrary permission.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to user query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetUserOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetUserOptionsQueryHandler> logger) :
    IQueryHandler<GetUserOptionsQuery, IReadOnlyList<UserOptionDto>>
{
    /// <summary>The permission each audit-author filter implies.</summary>
    private static readonly Dictionary<string, string> FieldPermissions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Domain.Entities.Identity.User.CreatedBy)] = Permissions.Users.Create,
            [nameof(Domain.Entities.Identity.User.UpdatedBy)] = Permissions.Users.Edit,
            [nameof(Domain.Entities.Identity.User.DeletedBy)] = Permissions.Users.Delete,
        };

    /// <summary>
    /// Returns the accounts able to perform the action named by the field, or a masked error
    /// when the read fails. An unknown field yields an empty list rather than the whole table.
    /// </summary>
    /// <param name="query">The query, carrying the filter field name.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<Result<IReadOnlyList<UserOptionDto>>> Handle(
        GetUserOptionsQuery query, CancellationToken ct)
    {
        if (!FieldPermissions.TryGetValue(query.Field ?? string.Empty, out var permissionKey))
        {
            logger.LogWarning("Requested user options for the unknown field {Field}.", query.Field);
            return Result<IReadOnlyList<UserOptionDto>>.Success([]);
        }

        try
        {
            var options = await unitOfWork.UserQueryRepository.GetUserOptionsAsync(permissionKey, ct);
            return Result<IReadOnlyList<UserOptionDto>>.Success(options);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading the user option list failed.");
            return Result<IReadOnlyList<UserOptionDto>>.Failure(UserErrors.ListFailed());
        }
    }
}
