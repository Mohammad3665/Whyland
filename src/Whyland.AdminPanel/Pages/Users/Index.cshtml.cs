using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.Users
{
    /// <summary>
    /// Users list page (section 4.1 of the UI/UX doc):
    /// authentication users with status filter, search, pagination and
    /// row actions (edit / view profile / toggle active status).
    /// </summary>
    public class IndexModel : PageModel
    {
        // TODO: replace the demo rows with real queries when the data layer is wired up.
        // Columns mirror the User entity: FirstName, LastName, UserName,
        // Email, PhoneNumber, IsActive, LastLoginAt, CreatedAt.
        public void OnGet()
        {
        }
    }
}
