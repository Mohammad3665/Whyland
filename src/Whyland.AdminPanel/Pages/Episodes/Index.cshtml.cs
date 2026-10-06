using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.Episodes
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int SectionId { get; set; } = 1;

        public string CourseTitle { get; private set; } = string.Empty;
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
