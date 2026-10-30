using Whyland.Domain.Common.Errors;

namespace Whyland.Domain.Entities.Content.Errors;

/// <summary>
/// Every failure the <see cref="Faq"/> aggregate can report, including the ones raised by the
/// handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs. The wording
/// lives in the localization catalog, so a failure never embeds user input in a hand-written sentence.
/// </para>
/// </summary>
public static class FaqErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<Faq>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<Faq>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The specified field carries a negative value where only zero or above is allowed.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError GreaterThanOrEqualToZero(string field) =>
        DomainError.For<Faq>(nameof(GreaterThanOrEqualToZero), ErrorType.Validation, field: field);

    /// <summary>No FAQ exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(long id) =>
        DomainError.For<Faq>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>No FAQ exists with the given title.</summary>
    /// <param name="title">The title that was looked up.</param>
    public static DomainError NotFoundByTitle(string title) =>
        DomainError.For<Faq>(nameof(NotFoundByTitle), ErrorType.NotFound, field: null, ("Title", title));

    /// <summary>The FAQ could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<Faq>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The FAQ could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<Faq>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The FAQ could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<Faq>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The FAQs could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<Faq>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The new display order could not be persisted (persistence failure); masked behind a generic message.</summary>
    public static DomainError ReorderFailed() =>
        DomainError.For<Faq>(nameof(ReorderFailed), ErrorType.Unexpected);
}
