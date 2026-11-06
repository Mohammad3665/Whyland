namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateRoleCommand"/> fields against the role domain constraints.
/// </summary>
public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    /// <summary>Configures the validation rules for every updatable role field.</summary>
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Name)
            .WhylandRequired()
            .WhylandMaxLen(Role.MaxNameLength)
            .WithName("نام نقش");

        RuleFor(x => x.Title)
            .WhylandRequired()
            .WhylandMaxLen(Role.MaxTitleLength)
            .WithName("عنوان نقش");
    }
}
