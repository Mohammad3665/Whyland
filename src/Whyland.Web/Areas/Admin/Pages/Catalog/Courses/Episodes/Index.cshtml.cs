
namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.Episodes
{
    /// <summary>
    /// Episodes list page: the episodes of a single course section,
    /// navigated to from the sections page (course → section → episode
    /// hierarchy), demo-only until the data layer is wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Id of the course whose episodes are being listed (demo lookup).</summary>
        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; } = 1;

        /// <summary>Id of the course section whose episodes are being listed (demo lookup).</summary>
        [BindProperty(SupportsGet = true)]
        public int SectionId { get; set; } = 1;

        /// <summary>Display title of the requested course, populated by OnGet from the demo lookup.</summary>
        public string CourseTitle { get; private set; } = string.Empty;

        /// <summary>Display title of the requested section, populated by OnGet from the demo lookup.</summary>
        public string SectionTitle { get; private set; } = string.Empty;

        // TODO: replace the demo lookups with real queries when the data layer is wired up.
        private static readonly Dictionary<int, string> DemoCourseTitles = new()
        {
            { 1, "دوره جامع ASP.NET Core" },
            { 2, "آموزش مقدماتی جاوااسکریپت" },
            { 3, "فتوشاپ از صفر تا صد" },
            { 4, "کارآفرینی و استارتاپ" },
        };

        private static readonly Dictionary<int, string> DemoSectionTitles = new()
        {
            { 1, "مقدمه و آماده‌سازی" },
            { 2, "مبانی C#" },
            { 3, "شیءگرایی (OOP) در C#" },
            { 4, "کار با دیتابیس و EF Core" },
        };

        /// <summary>Resolves the demo course and section titles for the requested ids so the view can show them while the data layer is not wired up.</summary>
        public void OnGet()
        {
            CourseTitle = DemoCourseTitles.TryGetValue(CourseId, out var course)
                ? course
                : $"دوره #{CourseId}";

            SectionTitle = DemoSectionTitles.TryGetValue(SectionId, out var section)
                ? section
                : $"سرفصل #{SectionId}";
        }
    }
}
