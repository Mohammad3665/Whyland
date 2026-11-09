namespace Whyland.Application.Features.Admin.Faqs.Queries.GetById;

/// <summary>
/// Validates <see cref="GetFaqByIdQuery"/> before executing the lookup.
/// </summary>
public sealed class GetFaqByIdQueryValidator : AbstractValidator<GetFaqByIdQuery>
{
    public GetFaqByIdQueryValidator()
    {
        RuleFor(x => x.Id.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه سوال");
    }
}
