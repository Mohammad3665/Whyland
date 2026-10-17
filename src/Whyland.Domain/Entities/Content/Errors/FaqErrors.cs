using Whyland.Domain.Common.Errors;
using Whyland.Shared.Errors;

namespace Whyland.Domain.Entities.Content.Errors;

/// <summary>
/// Domain errors produced by the <see cref="Faq"/> aggregate.
/// <para>
/// Each factory method builds a <see cref="DomainError"/> scoped to <see cref="Faq"/>
/// with a stable <c>code</c>, a <see cref="ErrorType.Validation"/> type, the offending
/// <c>field</c>, a localized description, and structured arguments for clients.
/// </para>
/// </summary>
public static class FaqErrors
{
    /// <summary>
    /// The specified field is required but was empty or whitespace.
    /// </summary>
    /// <param name="field">The name of the offending field.</param>
    /// <returns>A <see cref="ErrorType.Validation"/> domain error.</returns>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<Faq>(
            code: "FieldRequired",
            type: ErrorType.Validation,
            field: field,
            description: $"فیلد '{field}'  الزامی است.",
            arguments: ("Field", field));

    /// <summary>
    /// The specified field exceeds its maximum allowed length.
    /// </summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <returns>A <see cref="ErrorType.Validation"/> domain error.</returns>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<Faq>(
            code: "FieldTooLong",
            type: ErrorType.Validation,
            field: field,
            description: $"فیلد '{field}' نمیتواند بیشتر از '{max}' کاراکتر باشد.",
            ("Field", (object?)field), ("Max", (object?)max));

    /// <summary>
    /// The specified field has a value that is not valid in this context.
    /// </summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="reason">A short human-readable explanation of why the value is invalid.</param>
    /// <returns>A <see cref="ErrorType.Validation"/> domain error.</returns>
    public static DomainError InvalidValue(string field, string reason) =>
        DomainError.For<Faq>(
            code: "InvalidValue",
            type: ErrorType.Validation,
            field: field,
            description: $"فید '{field}' نامعتبر است. '{reason}'",
            ("Field", (object?)field), ("Reason", reason));
}