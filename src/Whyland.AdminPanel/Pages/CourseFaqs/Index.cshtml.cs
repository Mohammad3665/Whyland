using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.CourseFaqs
{
    /// <summary>
    /// Course FAQ list page (section 3.6 of the UI/UX doc):
    /// frequently asked questions of a single course, ordered by
    /// DisplayOrder (drag &amp; drop reordering supported).
    /// </summary>
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; } = 1;

        public string CourseTitle { get; private set; } = string.Empty;

        // TODO: replace the demo lookups with real queries when the data layer is wired up.
        private static readonly Dictionary<int, string> DemoCourseTitles = new()
        {
            { 1, "دوره جامع ASP.NET Core" },
            { 2, "آموزش مقدماتی جاوااسکریپت" },
            { 3, "فتوشاپ از صفر تا صد" },
            { 4, "کارآفرینی و استارتاپ" },
        };

        public void OnGet()
        {
            CourseTitle = DemoCourseTitles.TryGetValue(CourseId, out var course)
                ? course
                : $"دوره #{CourseId}";
        }
    }
}
