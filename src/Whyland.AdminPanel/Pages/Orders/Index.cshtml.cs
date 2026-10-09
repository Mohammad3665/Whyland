using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.Orders
{
    /// <summary>
    /// Orders list page (section 7.1 of the UI/UX doc): a DataTable of
    /// Order entities with status filter, search and a order-details
    /// modal (7.2 pattern), demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
