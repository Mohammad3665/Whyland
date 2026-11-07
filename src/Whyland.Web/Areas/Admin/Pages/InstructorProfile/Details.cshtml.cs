using Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetById;
using Whyland.Shared.Dtos.Identity.InstructorProfile;

namespace Whyland.Web.Areas.Admin.Pages.InstructorProfile;

/// <summary>Read-only profile page of one instructor, with the edit form opening on the list page.</summary>
[HasPermission(Permissions.Users.View)]
public class DetailsModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>The profile shown by the page.</summary>
    public InstructorProfileDetailsDto Profile { get; private set; } = null!;

    /// <summary>Loads the profile or answers 404 when it does not exist.</summary>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetInstructorProfileByIdQuery(id), ct);
        if (result.IsFailure)
            return NotFound();

        Profile = result.Data!;
        return Page();
    }
}
