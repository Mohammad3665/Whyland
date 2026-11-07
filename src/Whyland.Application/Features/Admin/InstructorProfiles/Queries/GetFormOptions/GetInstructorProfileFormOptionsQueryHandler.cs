namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetFormOptions;

/// <summary>Reads the eligible-user list from the query repository.</summary>
public sealed class GetInstructorProfileFormOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetInstructorProfileFormOptionsQueryHandler> logger)
    : IQueryHandler<GetInstructorProfileFormOptionsQuery, InstructorProfileFormOptionsDto>
{
    public async Task<Result<InstructorProfileFormOptionsDto>> Handle(
        GetInstructorProfileFormOptionsQuery query,
        CancellationToken ct)
    {
        try
        {
            var users = await unitOfWork.InstructorProfileQueryRepository.GetEligibleUsersAsync(ct);
            return Result<InstructorProfileFormOptionsDto>.Success(
                new InstructorProfileFormOptionsDto(users));
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading instructor form options failed.");
            return Result<InstructorProfileFormOptionsDto>.Failure(InstructorProfileErrors.OptionsFailed());
        }
    }
}
