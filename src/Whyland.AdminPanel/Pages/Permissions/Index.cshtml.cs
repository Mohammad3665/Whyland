using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.Permissions
{
    /// <summary>
    /// Permissions list page (sections 6.1 / 6.2 of the UI/UX doc):
    /// a DataTable of the dynamic Permission entities with create/edit
    /// modal, group filter (Key prefix) and delete confirmation with
    /// protection for system permissions.
    /// Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
