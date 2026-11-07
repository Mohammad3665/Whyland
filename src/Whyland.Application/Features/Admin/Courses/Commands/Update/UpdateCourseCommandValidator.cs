namespace Whyland.Application.Features.Admin.Courses.Commands.Update;

/// <summary>
/// Validates <see cref="UpdateCourseCommand"/> fields against the course domain constraints.
/// </summary>
public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    /// <summary>Configures the validation rules for every editable course field.</summary>
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .WhylandGreaterThan(0)
            .WithName("شناسه");

        RuleFor(x => x.Name)
            .WhylandRequired()
            .WhylandMaxLen(Course.MaxNameLength)
            .WithName("نام");

        RuleFor(x => x.LatinName)
            .WhylandRequired()
            .WhylandMaxLen(Course.MaxLatinNameLength)
            .WithName("نام لاتین");

        RuleFor(x => x.ShortDescription)
            .WhylandMaxLen(Course.MaxShortDescriptionLength)
            .WithName("توضیح کوتاه");

        RuleFor(x => x.Slug)
            .WhylandRequired()
            .WhylandMaxLen(Course.MaxSlugLength)
            .Matches(@"\A[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*\z")
            .WithMessage("اسلاگ فقط می‌تواند شامل حروف لاتین، عدد و خط تیره باشد.")
            .WithName("اسلاگ");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("قیمت باید بزرگ‌تر از صفر باشد؛ برای دورهٔ رایگان آن را خالی بگذارید.")
            .WithName("قیمت");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithName("نوع تخفیف");

        RuleFor(x => x.DisplayStatus)
            .IsInEnum()
            .WithName("وضعیت نمایش");

        RuleFor(x => x.CategoryIds)
            .NotNull()
            .WithMessage("دسته‌بندی‌های دوره را انتخاب کنید.")
            .WithName("دسته‌بندی‌ها");

        RuleForEach(x => x.CategoryIds)
            .GreaterThan(0)
            .WithName("شناسهٔ دسته‌بندی");

        RuleFor(x => x.InstructorIds)
            .NotNull()
            .NotEmpty()
            .WhylandWithError(CourseErrors.InstructorRequired())
            .WithName("مدرسین");

        RuleForEach(x => x.InstructorIds)
            .NotEmpty()
            .WithName("شناسهٔ مدرس");
    }
}
