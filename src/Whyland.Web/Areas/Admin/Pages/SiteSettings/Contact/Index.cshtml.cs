
namespace Whyland.Web.Areas.Admin.Pages.SiteSettings.Contact
{
    /// <summary>
    /// «اطلاعات تماس» page (sections 12.1 / 12.2 of the UI/UX doc):
    /// the contact SiteSetting group (phone, email, address) is rendered
    /// read-only first and edited through a modal.
    /// Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Demo last-update stamp shown in the card footer.</summary>
        public string LastUpdatedText { get; private set; } = string.Empty;

        /// <summary>Demo last-update author shown in the card footer.</summary>
        public string LastUpdatedBy { get; private set; } = string.Empty;

        public void OnGet()
        {
            // TODO: replace with the real SiteSetting rows (Group = "Contact").
            LastUpdatedText = "۱۴۰۵/۰۷/۱۲ - ۰۹:۳۰";
            LastUpdatedBy = "مدیر سیستم";
        }
    }
}
