namespace Whyland.Application.Features.Admin.Faqs.Commands.Delete;

/// <summary>
/// Validates <see cref="DeleteFaqCommand"/> fields before deletion.
/// </summary>
public sealed class DeleteFaqCommandValidator : AbstractValidator<DeleteFaqCommand>
{
    /// <summary>
    /// Configures the validation rules for the FAQ identifier.
    /// </summary>
    public DeleteFaqCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");
    }
}