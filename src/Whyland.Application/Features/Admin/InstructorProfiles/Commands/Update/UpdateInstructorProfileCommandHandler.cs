using Whyland.Domain.Model.Identity.InstructorProfile.Errors;

namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Update;

/// <summary>
/// Updates a tracked instructor profile, stores a replacement image or clears the current one,
/// and keeps old files from accumulating after a successful save.
/// </summary>
public sealed class UpdateInstructorProfileCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<UpdateInstructorProfileCommandHandler> logger) : ICommandHandler<UpdateInstructorProfileCommand>
{
    public async Task<Result> Handle(UpdateInstructorProfileCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.InstructorProfileCommandRepository;

        var profile = await repository.GetTrackedByIdAsync(command.Id, ct);
        if (profile is null)
            return InstructorProfileErrors.NotFound(command.Id);

        var updated = profile.Update(
            command.Bio,
            command.Biography,
            command.Specialization,
            InstructorProfileInputNormalizer.Url(command.Website),
            InstructorProfileInputNormalizer.Url(command.LinkedIn),
            InstructorProfileInputNormalizer.Url(command.Instagram),
            InstructorProfileInputNormalizer.Digits(command.ShebaNumber, stripIrPrefix: true),
            InstructorProfileInputNormalizer.Digits(command.BankAccountNumber));

        if (updated.IsFailure)
            return updated.Error;

        var oldImage = profile.ProfileImage;
        StoredImage? newImage = null;

        if (command.Image is not null)
        {
            var saved = await InstructorProfileImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            newImage = saved.Data!;

            var imageSet = profile.SetProfileImage(newImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await InstructorProfileImageStore.DeleteAsync(imageStorageService, newImage.RelativePath, ct);
                return imageSet.Error;
            }
        }
        else if (command.RemoveImage)
        {
            var cleared = profile.SetProfileImage(null);
            if (cleared.IsFailure)
                return cleared.Error;
        }

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
        {
            await InstructorProfileImageStore.DeleteAsync(imageStorageService, newImage?.RelativePath, ct);
            return InstructorProfileErrors.UpdateFailed();
        }

        if (newImage is not null || command.RemoveImage)
            await DeleteOldImageAsync(oldImage, ct);

        return Result.Success();
    }

    private async Task DeleteOldImageAsync(string? oldImage, CancellationToken ct)
    {
        var deleted = await InstructorProfileImageStore.DeleteAsync(imageStorageService, oldImage, ct);
        if (deleted.IsFailure)
            logger.LogWarning("The old instructor image '{Path}' could not be deleted.", oldImage);
    }
}
