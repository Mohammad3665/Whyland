namespace Whyland.Web.Areas.Admin.Pages.InstructorProfile
{
    /// <summary>
    /// Instructor profile management page (section 4.3 of the UI/UX doc):
    /// a single form for the instructor-specific InstructorProfile entity —
    /// image, bio, biography, specialization, social links, address and
    /// bank details. Demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Id of the instructor user being managed (demo ids mirror /Users/Index).</summary>
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; } = 1;

        /// <summary>Display name of the requested instructor, populated by OnGet from the demo lookup.</summary>
        public string InstructorName { get; private set; } = string.Empty;

        // TODO: replace the demo lookup with a real query when the data layer is wired up.
        private static readonly Dictionary<int, string> DemoInstructorNames = new()
        {
            [1] = "علی محمدی",
            [5] = "شیرین جعفری",
        };

        /// <summary>Resolves the demo instructor name for the requested user id so the view can show it while the data layer is not wired up.</summary>
        public void OnGet()
        {
            InstructorName = DemoInstructorNames.TryGetValue(UserId, out var name)
                ? name
                : $"مدرس #{UserId}";
        }
    }
}
