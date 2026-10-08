using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Whyland.AdminPanel.Pages.EpisodeAttachments
{
    /// <summary>
    /// Episode attachments list page (section 3.5 of the UI/UX doc):
    /// files attached to a single episode with upload restrictions
    /// (max count, max size, allowed extensions).
    /// </summary>
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int SectionId { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int EpisodeId { get; set; } = 1;

        public string CourseTitle { get; private set; } = string.Empty;
        public string SectionTitle { get; private set; } = string.Empty;
        public string EpisodeTitle { get; private set; } = string.Empty;

        // Upload restrictions — configuration-driven in the real system
        // (see SRS section 15: MaxAttachmentsPerEpisode, MaxAttachmentSizeBytes,
        // AllowedAttachmentExtensions). Rendered in the upload modal so the
        // admin sees the limits, and enforced server-side on upload.
        public static int MaxAttachmentsPerEpisodeLimit { get; } = 5;
        public static long MaxAttachmentSizeBytesLimit = 20 * 1024 * 1024; // 20 MB

        // Instance wrappers so the Razor views can read them via @Model.*.
        public int MaxAttachmentsPerEpisode => MaxAttachmentsPerEpisodeLimit;
        public long MaxAttachmentSizeBytes => MaxAttachmentSizeBytesLimit;
        public string[] AllowedAttachmentExtensions => AllowedAttachmentExtensionsList;

        private static readonly string[] AllowedAttachmentExtensionsList =
            { ".pdf", ".zip", ".rar", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };

        /// <summary>Human-readable max size for the modal hint (e.g. "20 مگابایت").</summary>
        public string MaxAttachmentSizeDisplay => $"{MaxAttachmentSizeBytes / (1024 * 1024)} مگابایت";

        /// <summary>Comma-separated allowed extensions for the modal hint.</summary>
        public string AllowedExtensionsDisplay => string.Join("، ", AllowedAttachmentExtensions);

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

        private static readonly Dictionary<int, string> DemoEpisodeTitles = new()
        {
            { 1, "معرفی دوره و نصب ابزارها" },
            { 2, "ساختار پروژه در ASP.NET Core" },
            { 3, "اجرای اولین برنامه" },
        };

        public void OnGet()
        {
            CourseTitle = DemoCourseTitles.TryGetValue(CourseId, out var course)
                ? course
                : $"دوره #{CourseId}";

            SectionTitle = DemoSectionTitles.TryGetValue(SectionId, out var section)
                ? section
                : $"سرفصل #{SectionId}";

            EpisodeTitle = DemoEpisodeTitles.TryGetValue(EpisodeId, out var episode)
                ? episode
                : $"اپیزود #{EpisodeId}";
        }
    }
}
