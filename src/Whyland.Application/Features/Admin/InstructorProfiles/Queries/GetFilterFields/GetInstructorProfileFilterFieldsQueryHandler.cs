using Whyland.Application.Common.Helpers;

namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetFilterFields;

/// <summary>
/// Builds the instructor picker from the profile entity and adds the joined account fields
/// that the list shows.
/// </summary>
public sealed class GetInstructorProfileFilterFieldsQueryHandler
    : IQueryHandler<GetInstructorProfileFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه پروفایل",
        ["UserId"] = "شناسه کاربر",
        ["Bio"] = "بیوگرافی کوتاه",
        ["Biography"] = "بیوگرافی کامل",
        ["Specialization"] = "تخصص",
        ["Website"] = "وب‌سایت",
        ["LinkedIn"] = "لینکدین",
        ["Instagram"] = "اینستاگرام",
        ["ShebaNumber"] = "شماره شبا",
        ["BankAccountNumber"] = "شماره حساب",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
    };

    private static readonly string[] Hidden =
    [
        "ProfileImage",
        "CourseInstructors",
        "CreatedBy",
        "UpdatedBy",
    ];

    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetInstructorProfileFilterFieldsQuery query,
        CancellationToken ct)
    {
        var fields = FilterFieldCatalog
            .Describe(typeof(InstructorProfile), Labels, Hidden)
            .ToList();

        fields.Insert(0, new FilterFieldDto("FirstName", "نام", "string", FilterFieldCatalog.Text));
        fields.Insert(1, new FilterFieldDto("LastName", "نام خانوادگی", "string", FilterFieldCatalog.Text));
        fields.Insert(2, new FilterFieldDto("Email", "ایمیل", "string", FilterFieldCatalog.Text));
        fields.Insert(3, new FilterFieldDto("PhoneNumber", "شماره موبایل", "string", FilterFieldCatalog.Text));
        fields.Add(new FilterFieldDto("CourseCount", "تعداد دوره", "number", FilterFieldCatalog.Range));

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }
}
