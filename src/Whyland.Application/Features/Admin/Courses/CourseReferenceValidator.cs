namespace Whyland.Application.Features.Admin.Courses;

/// <summary>
/// Checks the two references a course form submits — categories and instructors — against the
/// rows that can actually be selected. A category must be active and not deleted; an instructor
/// must still have an active account.
/// </summary>
internal static class CourseReferenceValidator
{
    /// <summary>
    /// Returns the first missing reference, or a successful result when every id resolves.
    /// </summary>
    public static async Task<Result> ValidateAsync(
        IUnitOfWork unitOfWork,
        IReadOnlyList<CategoryId> categoryIds,
        IReadOnlyList<InstructorProfileId> instructorIds,
        CancellationToken ct)
    {
        var requestedCategories = categoryIds.Distinct().ToList();
        if (requestedCategories.Count > 0)
        {
            var found = await unitOfWork.CourseQueryRepository
                .GetActiveCategoryIdsAsync(requestedCategories, ct);

            var missing = requestedCategories.FirstOrDefault(id => !found.Contains(id));
            if (missing.Value != 0)
                return CourseErrors.CategoryUnavailable(missing);
        }

        var requestedInstructors = instructorIds.Distinct().ToList();
        if (requestedInstructors.Count > 0)
        {
            var found = await unitOfWork.CourseQueryRepository
                .GetInstructorProfileIdsAsync(requestedInstructors, ct);

            var missing = requestedInstructors.FirstOrDefault(id => !found.Contains(id));
            if (missing.Value != Guid.Empty)
                return CourseErrors.InstructorUnavailable(missing);
        }

        return Result.Success();
    }
}
