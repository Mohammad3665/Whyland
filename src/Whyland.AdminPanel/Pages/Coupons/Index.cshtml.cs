using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.Coupons
{
    /// <summary>
    /// Coupons list page (sections 9.1–9.3 of the UI/UX doc): a DataTable
    /// of Coupon entities with create/edit modal, deactivate toggle,
    /// soft-delete confirmation and the derived coupon status
    /// (Active / Scheduled / Expired / Exhausted / Inactive).
    /// Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
