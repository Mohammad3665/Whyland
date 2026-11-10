using Whyland.Application.Common.Helpers;

namespace Whyland.Application.Features.Admin.BlogPosts.Queries.GetFilterFields;

/// <summary>
/// Handles <see cref="GetBlogPostFilterFieldsQuery"/> by reading the post properties through the
/// shared <see cref="FilterFieldCatalog"/> and adding the joined/computed columns.
/// A new scalar column shows up in the picker by itself — it only has to be in the query
/// repository's whitelist — so only its Persian label belongs here.
/// </summary>
public sealed class GetBlogPostFilterFieldsQueryHandler :
    IQueryHandler<GetBlogPostFilterFieldsQuery, IReadOnlyList<FilterFieldDto>>
{
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "شناسه",
        ["Title"] = "عنوان",
        ["ShortDescription"] = "توضیح کوتاه",
        ["Content"] = "محتوا",
        ["AuthorId"] = "نویسنده",
        ["AuthorName"] = "نام نویسنده",
        ["CategoryId"] = "دسته‌بندی",
        ["CategoryName"] = "نام دسته‌بندی",
        ["Image"] = "تصویر",
        ["DisplayOrder"] = "ترتیب نمایش",
        ["DisplayStatus"] = "وضعیت نمایش",
        ["Slug"] = "اسلاگ",
        ["CreatedAt"] = "تاریخ ایجاد",
        ["CreatedBy"] = "ایجاد شده توسط",
        ["UpdatedAt"] = "تاریخ آخرین ویرایش",
        ["UpdatedBy"] = "ویرایش شده توسط",
        ["DeletedAt"] = "تاریخ حذف",
        ["DeletedBy"] = "حذف شده توسط",
        ["DeletedByName"] = "نام حذف‌کننده",
        ["IsDeleted"] = "حذف‌شده",
    };

    /// <summary>
    /// Computed or joined columns that are not CLR properties of the entity. Their names must match
    /// the extras whitelisted by <c>BlogPostQueryRepository</c>.
    /// </summary>
    private static readonly FilterFieldDto[] Computed =
    [
        new("AuthorName", Labels["AuthorName"], "string", FilterFieldCatalog.Text),
        new("CategoryName", Labels["CategoryName"], "string", FilterFieldCatalog.Text),
        new("DeletedByName", Labels["DeletedByName"], "string", FilterFieldCatalog.Text),
    ];

    /// <summary>
    /// Returns the filterable fields of a blog post: the entity's scalar properties, then the
    /// joined/computed columns. The category and author ids are exposed as pickers fed by the same
    /// option lists the form uses, instead of asking an operator to paste a numeric id or a guid.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public Task<Result<IReadOnlyList<FilterFieldDto>>> Handle(
        GetBlogPostFilterFieldsQuery query, CancellationToken ct)
    {
        var fields = FilterFieldCatalog
            .Describe(typeof(BlogPost), Labels, [])
            .ToList();

        ReplaceKind(fields, nameof(BlogPost.AuthorId), "blogAuthor");
        ReplaceKind(fields, nameof(BlogPost.CategoryId), "blogCategory");

        fields.AddRange(Computed);

        return Task.FromResult(Result<IReadOnlyList<FilterFieldDto>>.Success(fields));
    }

    /// <summary>
    /// Replaces one described field with the same metadata under a page-specific picker kind. The
    /// property name and operators stay identical; only the value control changes.
    /// </summary>
    private static void ReplaceKind(
        List<FilterFieldDto> fields, string property, string kind)
    {
        var index = fields.FindIndex(x =>
            string.Equals(x.Property, property, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            return;

        fields[index] = fields[index] with
        {
            Kind = kind,
            Operators = FilterFieldCatalog.List,
        };
    }
}
