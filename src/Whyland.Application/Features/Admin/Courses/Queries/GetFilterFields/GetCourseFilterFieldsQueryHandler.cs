using Whyland.Application.Common.Helpers;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetCourseFilterFieldsQuery"/> by reading the course properties through the
/// shared <see cref="FilterFieldCatalog"/> and adding the two membership filters.
/// </summary>
public sealed class GetCourseFilterFieldsQueryHandler :
    IQueryHandler<GetCourseFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
    /// <summary>Persian labels for the course fields.</summary>
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه",
        ["Name"] = "نام",
        ["LatinName"] = "نام لاتین",
        ["ShortDescription"] = "توضیح کوتاه",
        ["Description"] = "توضیحات",
        ["Slug"] = "اسلاگ",
        ["MainImage"] = "تصویر اصلی",
        ["Price"] = "قیمت",
        ["DiscountType"] = "نوع تخفیف",
        ["DiscountValue"] = "مقدار تخفیف",
        ["IsSaleActive"] = "فروش فعال",
        ["DisplayStatus"] = "وضعیت نمایش",
        ["IsFeatured"] = "ویژه",
        ["IsAmazing"] = "شگفت‌انگیز",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["CreatedBy"] = "ایجاد شده توسط",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
        ["UpdatedBy"] = "ویرایش شده توسط",
    };

    /// <summary>
    /// Computed or non-column properties that must never be offered: a filter on them would name a
    /// column the table does not have. Soft-delete bookkeeping is hidden because deleted courses
    /// never appear in this listing.
    /// </summary>
    private static readonly string[] Hidden =
    [
        "MainImage",
        "MainImageUrl",
        "ThumbnailUrl",
        "IsFree",
        "IsPaid",
        "IsDeleted",
        "DeletedAt",
        "DeletedBy",
    ];

    /// <summary>
    /// Returns the filterable fields of a course, then the category and instructor membership
    /// filters the query repository whitelists by hand.
    /// </summary>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetCourseFilterFieldsQuery query,
        CancellationToken ct)
    {
        var fields = FilterFieldCatalog
            .Describe(typeof(Course), Labels, Hidden)
            .ToList();

        // These two are not properties of the row: the repository resolves them against the link
        // tables. Their kinds tell the picker to fill the value box from the categories and
        // instructors the page already loaded for its form.
        fields.Insert(0, new FilterFieldDto("CategoryId", "دسته‌بندی", "courseCategory", FilterFieldCatalog.List));
        fields.Insert(1, new FilterFieldDto("InstructorProfileId", "مدرس", "courseInstructor", FilterFieldCatalog.List));

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }
}
