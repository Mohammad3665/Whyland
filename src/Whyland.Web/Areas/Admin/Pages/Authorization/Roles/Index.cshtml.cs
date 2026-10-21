
namespace Whyland.Web.Areas.Admin.Pages.Authorization.Roles
{
    /// <summary>
    /// Roles list page (section 5.1 of the UI/UX doc): a DataTable of the
    /// dynamic Role entities with create/edit modal (5.2), delete
    /// confirmation and links to role-permission management.
    /// Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Renders the demo roles list; no data layer wired up yet.</summary>
        public void OnGet() { }
    }
}
