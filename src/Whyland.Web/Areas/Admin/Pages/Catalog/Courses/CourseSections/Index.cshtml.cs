
namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.CourseSections
{
    /// <summary>
    /// Course sections list page: the sections of a single course,
    /// ordered by DisplayOrder (drag &amp; drop reordering supported),
    /// parent of the Episodes page, demo-only until the data layer is
    /// wired up.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>Id of the course whose sections are being listed (demo lookup).</summary>
        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; } = 1;

        /// <summary>Display title of the requested course, populated by OnGet from the demo lookup.</summary>
        public string CourseTitle { get; private set; } = string.Empty;

        // TODO: replace the demo lookup with a real query when the data layer is wired up.
        private static readonly Dictionary<int, string> DemoCourseTitles = new()
        {
            { 1, "دوره جامع ASP.NET Core" },
            { 2, "آموزش مقدماتی جاوااسکریپت" },
            { 3, "فتوشاپ از صفر تا صد" },
            { 4, "کارآفرینی و استارتاپ" },
        };

        /// <summary>Resolves the demo course title for the requested course id so the view can show it while the data layer is not wired up.</summary>
        public void OnGet()
        {
            CourseTitle = DemoCourseTitles.TryGetValue(CourseId, out var title)
                ? title
                : $"دوره #{CourseId}";
        }
    }
}
