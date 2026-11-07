using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Domain.Model.Catalog.Course.Errors;

namespace Whyland.Application.Features.Admin.Courses.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteCourseCommand"/> by soft-deleting the course through the command
/// repository and removing its cover image once the row is saved.
/// </summary>
public sealed class DeleteCourseCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<DeleteCourseCommandHandler> logger) : ICommandHandler<DeleteCourseCommand>
{
    /// <summary>
    /// Soft-deletes the requested course, or returns an error when it does not exist or persistence
    /// fails. Removing the image is best effort: a file-system failure must not resurrect a deleted
    /// course.
    /// </summary>
    public async Task<Result> Handle(DeleteCourseCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.CourseCommandRepository;

        var course = await repository.GetByIdAsync(command.Id, ct);
        if (course is null)
            return CourseErrors.NotFound(command.Id);

        var image = course.MainImage;

        repository.SoftDelete(course);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseErrors.SoftDeleteFailed();

        var deleted = await CourseImageStore.DeleteAsync(imageStorageService, image, ct);
        if (deleted.IsFailure)
            logger.LogWarning("The cover image of deleted course {CourseId} could not be removed.", command.Id);

        return Result.Success();
    }
}
