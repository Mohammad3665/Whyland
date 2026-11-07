using Whyland.Domain.Common.Errors;

using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.Course.Errors;

/// <summary>
/// Every failure the <see cref="CourseEntity"/> aggregate can report, including the ones raised by
/// the handlers that save or read it. Each factory carries the code and the arguments the message
/// needs; the wording lives in the localization catalog.
/// </summary>
public static class CourseErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<CourseEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<CourseEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The slug is not in the URL-safe lower-case kebab shape.</summary>
    public static DomainError InvalidSlug() =>
        DomainError.For<CourseEntity>(nameof(InvalidSlug), ErrorType.Validation, nameof(CourseEntity.Slug));

    /// <summary>A paid course must carry a price greater than zero.</summary>
    public static DomainError InvalidPrice() =>
        DomainError.For<CourseEntity>(nameof(InvalidPrice), ErrorType.Validation, nameof(CourseEntity.Price));

    /// <summary>The discount value does not fit its type or the price.</summary>
    public static DomainError InvalidDiscount() =>
        DomainError.For<CourseEntity>(nameof(InvalidDiscount), ErrorType.Validation, nameof(CourseEntity.DiscountValue));

    /// <summary>A fixed-amount discount needs a paid course to subtract from.</summary>
    public static DomainError DiscountRequiresPrice() =>
        DomainError.For<CourseEntity>(nameof(DiscountRequiresPrice), ErrorType.Validation, nameof(CourseEntity.DiscountValue));

    /// <summary>The display status is not one of the known values.</summary>
    public static DomainError InvalidDisplayStatus() =>
        DomainError.For<CourseEntity>(nameof(InvalidDisplayStatus), ErrorType.Validation, nameof(CourseEntity.DisplayStatus));

    /// <summary>No course exists with the given identifier.</summary>
    public static DomainError NotFound(long id) =>
        DomainError.For<CourseEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>Another course already uses this slug.</summary>
    public static DomainError SlugTaken(string slug) =>
        DomainError.For<CourseEntity>(nameof(SlugTaken), ErrorType.Validation, nameof(CourseEntity.Slug), ("Slug", slug));

    /// <summary>A category the form selected is missing, deleted or inactive.</summary>
    public static DomainError CategoryUnavailable(long id) =>
        DomainError.For<CourseEntity>(nameof(CategoryUnavailable), ErrorType.Validation, field: null, ("Id", id));

    /// <summary>An instructor the form selected is missing or belongs to an inactive account.</summary>
    public static DomainError InstructorUnavailable(Guid id) =>
        DomainError.For<CourseEntity>(nameof(InstructorUnavailable), ErrorType.Validation, field: null, ("Id", id));

    /// <summary>A course must have at least one instructor.</summary>
    public static DomainError InstructorRequired() =>
        DomainError.For<CourseEntity>(nameof(InstructorRequired), ErrorType.Validation, field: "InstructorIds");

    /// <summary>The course could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<CourseEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The course could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<CourseEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The course could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<CourseEntity>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The courses could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<CourseEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The requested course could not be read (persistence failure).</summary>
    public static DomainError DetailsFailed() =>
        DomainError.For<CourseEntity>(nameof(DetailsFailed), ErrorType.Unexpected);

    /// <summary>The category and instructor selectors could not be read (persistence failure).</summary>
    public static DomainError OptionsFailed() =>
        DomainError.For<CourseEntity>(nameof(OptionsFailed), ErrorType.Unexpected);
}
