using Whyland.Domain.Model.Identity.InstructorProfile.Errors;

namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Create;

/// <summary>
/// Creates an instructor profile after checking the account, storing the optional image and
/// persisting the aggregate.
/// </summary>
public sealed class CreateInstructorProfileCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<CreateInstructorProfileCommandHandler> logger)
    : ICommandHandler<CreateInstructorProfileCommand, InstructorProfileId>
{
    public async Task<Result<InstructorProfileId>> Handle(
        CreateInstructorProfileCommand command,
        CancellationToken ct)
    {
        var user = await unitOfWork.UserCommandRepository.GetTrackedByIdAsync(command.UserId, ct);
        if (user is null)
            return InstructorProfileErrors.UserNotFound(command.UserId);

        if (!user.IsActive)
            return InstructorProfileErrors.UserInactive(command.UserId);

        var existing = await unitOfWork.InstructorProfileCommandRepository
            .GetTrackedByUserIdAsync(command.UserId, ct);
        if (existing is not null)
            return InstructorProfileErrors.AlreadyExists(command.UserId);

        var created = InstructorProfile.Create(
            UserId.From(command.UserId.Value),
            command.Bio,
            command.Biography,
            command.Specialization,
            InstructorProfileInputNormalizer.Url(command.Website),
            InstructorProfileInputNormalizer.Url(command.LinkedIn),
            InstructorProfileInputNormalizer.Url(command.Instagram),
            InstructorProfileInputNormalizer.Digits(command.ShebaNumber, stripIrPrefix: true),
            InstructorProfileInputNormalizer.Digits(command.BankAccountNumber));

        if (created.IsFailure)
            return created.Error;

        var profile = created.Data!;
        StoredImage? storedImage = null;

        if (command.Image is not null)
        {
            var saved = await InstructorProfileImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            storedImage = saved.Data!;

            var imageSet = profile.SetProfileImage(storedImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await InstructorProfileImageStore.DeleteAsync(imageStorageService, storedImage.RelativePath, ct);
                return imageSet.Error;
            }
        }

        try
        {
            await unitOfWork.InstructorProfileCommandRepository.AddAsync(profile, ct);

            var saveResult = await unitOfWork.SaveAsync(ct);
            if (saveResult.IsFailure)
            {
                await InstructorProfileImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);
                return InstructorProfileErrors.CreateFailed();
            }

            return profile.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Creating instructor profile for user {UserId} failed.", command.UserId);

            await InstructorProfileImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);
            return InstructorProfileErrors.CreateFailed();
        }
    }
}
