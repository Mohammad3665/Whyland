namespace Whyland.Web.Areas.Admin.Pages.Commerce.Payments
{
    /// <summary>
    /// Payments list page (sections 8.1 / 8.2 of the UI/UX doc): a
    /// read-only DataTable of Payment entities with status/method filters
    /// and a payment-details modal, demo-only until the data layer is
    /// wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Renders the demo payments list; no data layer wired up yet.</summary>
        public void OnGet() { }
    }
}
