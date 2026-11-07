using Whyland.Application.Features.Admin.InstructorProfiles;
using Whyland.Application.Features.Admin.InstructorProfiles.Commands.Create;
using Whyland.Application.Features.Admin.InstructorProfiles.Commands.Update;
using Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetById;
using Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetFormOptions;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.InstructorProfile;

/// <summary>
/// «مدرسین» admin page. Lists the teaching profiles, shows their full profile page and creates
/// or edits the profile of an existing account. The course form can only select accounts that
/// have a profile created here.
/// </summary>
[HasPermission(Permissions.Users.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    public void OnGet()
    {
    }

    /// <summary>Returns one page of instructor profiles for the server-side table.</summary>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter,
        List<FilterCondition>? filters,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filter.SortColumn))
        {
            filter.SortColumn = "createdAt";
            filter.SortColumnDirection = "desc";
        }

        var result = await dispatcher.Send(
            new GetInstructorProfilesForDataTableQuery(filter, filters), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>Returns the fields the generic filter picker can build conditions for.</summary>
    public async Task<IActionResult> OnGetFieldsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetInstructorProfileFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>Returns active accounts that do not have an instructor profile yet.</summary>
    public async Task<IActionResult> OnGetFormOptionsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetInstructorProfileFormOptionsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>Returns one instructor profile for the edit form.</summary>
    public async Task<IActionResult> OnGetDetailsAsync(Guid id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetInstructorProfileByIdQuery(id), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    #region Mutations

    /// <summary>Creates an instructor profile from the full-screen form.</summary>
    public async Task<IActionResult> OnPostCreateAsync(
        [FromForm] InstructorProfileFormInput input,
        CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Users.Create);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            var image = ReadImage(input.Image, out uploadStream);
            var command = new CreateInstructorProfileCommand(
                input.UserId,
                input.Bio,
                input.Biography,
                input.Specialization,
                input.Website,
                input.LinkedIn,
                input.Instagram,
                input.ShebaNumber,
                input.BankAccountNumber,
                image);

            return ToJson(await dispatcher.Send(command, ct));
        }
        finally
        {
            if (uploadStream is not null)
                await uploadStream.DisposeAsync();
        }
    }

    /// <summary>Updates an instructor profile from the full-screen form.</summary>
    public async Task<IActionResult> OnPostUpdateAsync(
        [FromForm] InstructorProfileFormInput input,
        CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Users.Edit);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            var image = ReadImage(input.Image, out uploadStream);
            var command = new UpdateInstructorProfileCommand(
                input.Id,
                input.Bio,
                input.Biography,
                input.Specialization,
                input.Website,
                input.LinkedIn,
                input.Instagram,
                input.ShebaNumber,
                input.BankAccountNumber,
                input.RemoveImage,
                image);

            return ToJson(await dispatcher.Send(command, ct));
        }
        finally
        {
            if (uploadStream is not null)
                await uploadStream.DisposeAsync();
        }
    }

    #endregion

    /// <summary>Adapts an upload to the application contract and returns the stream to dispose.</summary>
    private static InstructorProfileImageUpload? ReadImage(
        IFormFile? file,
        out Stream? uploadStream)
    {
        uploadStream = null;

        if (file is not { Length: > 0 })
            return null;

        uploadStream = file.OpenReadStream();
        return new InstructorProfileImageUpload(
            uploadStream,
            file.FileName,
            file.ContentType,
            file.Length);
    }

    /// <summary>The multipart form shape the full-screen instructor form posts.</summary>
    public sealed class InstructorProfileFormInput
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Bio { get; set; }
        public string? Biography { get; set; }
        public string? Specialization { get; set; }
        public string? Website { get; set; }
        public string? LinkedIn { get; set; }
        public string? Instagram { get; set; }
        public string? ShebaNumber { get; set; }
        public string? BankAccountNumber { get; set; }
        public bool RemoveImage { get; set; }
        public IFormFile? Image { get; set; }
    }
}
