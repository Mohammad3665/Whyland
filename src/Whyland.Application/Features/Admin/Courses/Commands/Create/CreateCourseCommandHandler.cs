using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Domain.Model.Catalog.Course.Errors;

namespace Whyland.Application.Features.Admin.Courses.Commands.Create;

/// <summary>
/// Handles <see cref="CreateCourseCommand"/> by checking the cross-row references and slug, storing
/// the optional cover image, creating the aggregate through its domain factory, and persisting the
/// course and its links in one transaction.
/// </summary>
public sealed class CreateCourseCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<CreateCourseCommandHandler> logger) : ICommandHandler<CreateCourseCommand, long>
{
    /// <summary>
    /// Creates the course, or returns an error when a reference is unavailable, the slug is taken,
    /// the domain rejects the values, the image cannot be stored, or persistence fails.
    /// </summary>
    public async Task<Result<long>> Handle(CreateCourseCommand command, CancellationToken ct)
    {
        var references = await CourseReferenceValidator.ValidateAsync(
            unitOfWork, command.CategoryIds, command.InstructorIds, ct);
        if (references.IsFailure)
            return references.Error;

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await unitOfWork.CourseCommandRepository.IsSlugTakenAsync(slug, ct: ct))
            return CourseErrors.SlugTaken(slug);

        var created = Course.Create(
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

        if (created.IsFailure)
            return created.Error;

        var course = created.Data!;
        StoredImage? storedImage = null;

        if (command.Image is not null)
        {
            var saved = await CourseImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            storedImage = saved.Data!;

            var imageSet = course.SetMainImage(storedImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await CourseImageStore.DeleteAsync(imageStorageService, storedImage.RelativePath, ct);
                return imageSet.Error;
            }
        }

        course.ReplaceCategories(command.CategoryIds);
        course.ReplaceInstructors(command.InstructorIds);

        try
        {
            await unitOfWork.CourseCommandRepository.AddAsync(course, ct);

            // A single SaveChanges is already atomic, and Npgsql's retrying execution strategy
            // rejects a user-initiated transaction around it.
            var saveResult = await unitOfWork.SaveAsync(ct);
            if (saveResult.IsFailure)
            {
                await CourseImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);
                return CourseErrors.CreateFailed();
            }

            return course.Id.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Creating course '{Slug}' failed.", slug);

            await CourseImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);

            return CourseErrors.CreateFailed();
        }
    }
}
