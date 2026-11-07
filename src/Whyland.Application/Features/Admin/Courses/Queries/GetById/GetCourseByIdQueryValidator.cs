namespace Whyland.Application.Features.Admin.Courses.Queries.GetById;

/// <summary>Validates <see cref="GetCourseByIdQuery"/>.</summary>
public sealed class GetCourseByIdQueryValidator : AbstractValidator<GetCourseByIdQuery>
{
    /// <summary>Configures the validation rule for the course identifier.</summary>
    public GetCourseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}
