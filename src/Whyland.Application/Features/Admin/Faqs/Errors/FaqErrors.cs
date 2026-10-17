using Whyland.Domain.Common.Errors;

namespace Whyland.Application.Features.Admin.Faqs.Errors;

/// <summary>
/// Central catalog of <see cref="Error"/> instances produced by the FAQ feature.
/// <para>
/// Failures that indicate an unexpected infrastructure/persistence problem
/// (create, update, soft-delete) use <see cref="ErrorType.Unexpected"/> and are
/// meant to be logged, then masked behind a generic message toward the client.
/// Lookup failures use <see cref="ErrorType.NotFound"/> and may be surfaced to the user.
/// </para>
/// </summary>
public static class FaqErrors
{
    /// <summary>
    /// The FAQ could not be created (typically a persistence failure).
    /// </summary>
    /// <param name="title">The title of the FAQ that failed to be created.</param>
    /// <returns>An <see cref="ErrorType.Unexpected"/> error describing the failure.</returns>
    public static Error CreateFailed(string title) => new(
        "Faq.CreateFailed",
        $"خطا در ایجاد سوال {title}٬ لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// The FAQ could not be updated (typically a persistence failure).
    /// </summary>
    /// <param name="title">The title of the FAQ that failed to be updated.</param>
    /// <returns>An <see cref="ErrorType.Unexpected"/> error describing the failure.</returns>
    public static Error UpdateFailed(string title) => new(
        "Faq.UpdateFailed",
        $"خطا در ویرایش سوال {title}٬ لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// The FAQ could not be soft-deleted (typically a persistence failure).
    /// </summary>
    /// <param name="id">The identifier of the FAQ that failed to be soft-deleted.</param>
    /// <returns>An <see cref="ErrorType.Unexpected"/> error describing the failure.</returns>
    public static Error SoftDeleteFailed(long id) => new(
        "Faq.SoftDeleteFailed",
        $"خطا در حذف نرم سوال با شناسه {id}٬ لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// No FAQ was found with the given title.
    /// </summary>
    /// <param name="title">The title that was searched for.</param>
    /// <returns>An <see cref="ErrorType.NotFound"/> error.</returns>
    public static Error NotFound(string title) => new(
        "Faq.NotFound",
        $"سوالی با عنوان {title} یافت نشد.",
        ErrorType.NotFound
    );

    /// <summary>
    /// The FAQ list could not be read (typically a persistence failure).
    /// </summary>
    /// <returns>An <see cref="ErrorType.Unexpected"/> error describing the failure.</returns>
    public static Error ListFailed() => new(
        "Faq.ListFailed",
        "خطا در دریافت لیست سوالات٬ لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// The new FAQ display order could not be persisted (typically a persistence failure).
    /// </summary>
    /// <returns>An <see cref="ErrorType.Unexpected"/> error describing the failure.</returns>
    public static Error ReorderFailed() => new(
        "Faq.ReorderFailed",
        "خطا در ذخیره ترتیب سوالات٬ لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// No FAQ was found with the given identifier.
    /// </summary>
    /// <param name="id">The identifier that was searched for.</param>
    /// <returns>An <see cref="ErrorType.NotFound"/> error.</returns>
    public static Error NotFound(long id) => new(
        "Faq.NotFound",
        $"سوالی با شناسه {id} یافت نشد.",
        ErrorType.NotFound
    );
}