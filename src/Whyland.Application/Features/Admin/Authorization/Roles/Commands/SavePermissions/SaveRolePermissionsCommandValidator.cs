namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.SavePermissions;

/// <summary>
/// Validates <see cref="SaveRolePermissionsCommand"/>: the identifiers arrive as raw numbers from the
/// permission tree, so each one has to be a real key before the handler compares it with the catalogue.
/// </summary>
public sealed class SaveRolePermissionsCommandValidator : AbstractValidator<SaveRolePermissionsCommand>
{
    /// <summary>Configures the validation rules for the grant set.</summary>
    public SaveRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId.Value)
            .WhylandGreaterThan(0)
            .WithName("شناسه نقش");

        RuleFor(x => x.PermissionIds)
            .NotNull()
            .WithName("دسترسی‌ها");

        RuleForEach(x => x.PermissionIds.Select(id => id.Value))
            .WhylandGreaterThan(0)
            .WithName("دسترسی");
    }
}
