namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetById;

/// <summary>Reads one instructor profile or reports it as missing.</summary>
public sealed class GetInstructorProfileByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetInstructorProfileByIdQueryHandler> logger)
    : IQueryHandler<GetInstructorProfileByIdQuery, InstructorProfileDetailsDto>
{
    public async Task<Result<InstructorProfileDetailsDto>> Handle(
        GetInstructorProfileByIdQuery query,
        CancellationToken ct)
    {
        try
        {
            var details = await unitOfWork.InstructorProfileQueryRepository.GetByIdAsync(query.Id, ct);
            return details is null
                ? Result<InstructorProfileDetailsDto>.Failure(InstructorProfileErrors.NotFound(query.Id))
                : Result<InstructorProfileDetailsDto>.Success(details);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading instructor profile {ProfileId} failed.", query.Id);
            return Result<InstructorProfileDetailsDto>.Failure(InstructorProfileErrors.DetailsFailed());
        }
    }
}
