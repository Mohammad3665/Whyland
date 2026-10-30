namespace Whyland.Web.Areas.Admin.Pages.Commerce.Orders
{
    /// <summary>
    /// Orders list page (section 7.1 of the UI/UX doc): a DataTable of
    /// Order entities with status filter, search and a order-details
    /// modal (7.2 pattern), demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Renders the demo orders list; no data layer wired up yet.</summary>
        public void OnGet() { }
    }
}
