namespace Whyland.Web.Areas.Admin.Pages.SiteSettings.General
{
    /// <summary>
    /// «تنظیمات عمومی» page (sections 12.1 / 12.2 of the UI/UX doc):
    /// the general SiteSetting group (site title, logo, favicon, description)
    /// is rendered read-only first and edited through a modal.
    /// Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Demo last-update stamp shown in the card footer.</summary>
        public string LastUpdatedText { get; private set; } = string.Empty;

        /// <summary>Demo last-update author shown in the card footer.</summary>
        public string LastUpdatedBy { get; private set; } = string.Empty;

        /// <summary>Populates the demo last-update footer values while the data layer is not wired up.</summary>
        public void OnGet()
        {
            // TODO: replace with the real SiteSetting rows (Group = "General").
            LastUpdatedText = "۱۴۰۵/۰۷/۲۰ - ۱۰:۲۴";
            LastUpdatedBy = "مدیر سیستم";
        }
    }
}
