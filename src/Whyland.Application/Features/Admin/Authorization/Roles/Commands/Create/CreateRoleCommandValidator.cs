namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.Create;

/// <summary>
/// Validates the create-role modal. The two fields are mandatory and bounded by the column sizes the
/// entity declares, so the request cannot ask for a value the store would reject later.
/// </summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    /// <summary>Configures the validation rules for every new role field.</summary>
    public CreateRoleCommandValidator()
    {
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
