using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Domain.Model.Catalog.Course.Errors;

namespace Whyland.Application.Features.Admin.Courses.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateCourseCommand"/> by loading the tracked course, checking the
/// cross-row references and slug, applying the domain update and the link replacement, storing a
/// new cover image when one was uploaded, and persisting the result.
/// </summary>
public sealed class UpdateCourseCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<UpdateCourseCommandHandler> logger) : ICommandHandler<UpdateCourseCommand>
{
    /// <summary>
    /// Updates the requested course, or returns an error when it no longer exists, a reference is
    /// unavailable, the slug is taken, the domain rejects the values, the image cannot be stored,
    /// or persistence fails.
    /// </summary>
    public async Task<Result> Handle(UpdateCourseCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.CourseCommandRepository;

        var course = await repository.GetByIdAsync(command.Id, ct);
        if (course is null)
            return CourseErrors.NotFound(command.Id);

        var references = await CourseReferenceValidator.ValidateAsync(
            unitOfWork, command.CategoryIds, command.InstructorIds, ct);
        if (references.IsFailure)
            return references.Error;

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await repository.IsSlugTakenAsync(slug, command.Id, ct))
            return CourseErrors.SlugTaken(slug);

        var updated = course.Update(
            command.Name,
            command.LatinName,
            command.ShortDescription,
            command.Description,
            slug,
            command.Price,
            command.DiscountType,
            command.DiscountValue,
            command.IsSaleActive,
            command.DisplayStatus,
            command.IsFeatured,
            command.IsAmazing);

        if (updated.IsFailure)
            return updated.Error;

        var oldImage = course.MainImage;
        StoredImage? newImage = null;

        if (command.Image is not null)
        {
            var saved = await CourseImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            newImage = saved.Data!;

            var imageSet = course.SetMainImage(newImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await CourseImageStore.DeleteAsync(imageStorageService, newImage.RelativePath, ct);
                return imageSet.Error;
            }
        }
        else if (command.RemoveImage)
        {
            var imageCleared = course.SetMainImage(null);
            if (imageCleared.IsFailure)
                return imageCleared.Error;
        }

        course.ReplaceCategories(command.CategoryIds);
        course.ReplaceInstructors(command.InstructorIds);

        repository.Update(course);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
        {
            await CourseImageStore.DeleteAsync(imageStorageService, newImage?.RelativePath, ct);
            return CourseErrors.UpdateFailed();
        }

        // The row now points at the new file (or at nothing), so the previous file is unreachable.
        // A failure here costs disk space, never data, so it is logged and the save stands.
        if (newImage is not null || command.RemoveImage)
            await DeleteOldImageAsync(oldImage, ct);

        return Result.Success();
    }

    private async Task DeleteOldImageAsync(string? oldImage, CancellationToken ct)
    {
        var deleted = await CourseImageStore.DeleteAsync(imageStorageService, oldImage, ct);
        if (deleted.IsFailure)
            logger.LogWarning("The old cover image '{Path}' could not be deleted.", oldImage);
    }
}
