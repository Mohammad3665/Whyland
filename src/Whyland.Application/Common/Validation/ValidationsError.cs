using Whyland.Domain.Common.Errors;

namespace Whyland.Application.Common.Validation;

/// <summary>
/// Central catalog of validation <see cref="Error"/> instances.
/// Each error carries a stable code, a human-readable message (with optional
/// placeholders such as <c>{PropertyName}</c> or <c>{MaxLength}</c>),
/// and the <see cref="ErrorType.Validation"/> type.
/// </summary>
public static class ValidationErrors
{
    /// <summary>
    /// Generic validation failure used when multiple field failures are
    /// aggregated into a single error.
    /// </summary>
    public static readonly Error Failed = new(
        "Validation.Failed", "اعتبارسنجی ناموفق بود.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> is required and cannot be empty.
    /// </summary>
    public static readonly Error Required = new(
        "Validation.Required", "فیلد '{PropertyName}' نمی‌تواند خالی باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> exceeds the maximum length <c>{MaxLength}</c>.
    /// </summary>
    public static readonly Error TooLong = new(
        "Validation.TooLong", "فیلد '{PropertyName}' نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> is shorter than the minimum length <c>{MinLength}</c>.
    /// </summary>
    public static readonly Error TooShort = new(
        "Validation.TooShort", "فیلد '{PropertyName}' نمی‌تواند کمتر از {MinLength} کاراکتر باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> must be a valid email address.
    /// </summary>
    public static readonly Error InvalidEmail = new(
        "Validation.InvalidEmail", "فیلد '{PropertyName}' باید یک ایمیل معتبر باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> has an invalid format.
    /// </summary>
    public static readonly Error InvalidFormat = new(
        "Validation.InvalidFormat", "فرمت فیلد '{PropertyName}' معتبر نیست.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> must be greater than <c>{ComparisonValue}</c>.
    /// </summary>
    public static readonly Error GreaterThan = new(
        "Validation.GreaterThan", "فیلد '{PropertyName}' باید بزرگ‌تر از {ComparisonValue} باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> must be greater than or equal to <c>{ComparisonValue}</c>.
    /// </summary>
    public static readonly Error GreaterThanOrEqual = new(
        "Validation.GreaterThanOrEqual", "فیلد '{PropertyName}' باید بزرگ‌تر یا مساوی با {ComparisonValue} باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> must be less than <c>{ComparisonValue}</c>.
    /// </summary>
    public static readonly Error LessThan = new(
        "Validation.LessThan", "فیلد '{PropertyName}' باید کمتر از {ComparisonValue} باشد.", ErrorType.Validation);

    /// <summary>
    /// The field <c>{PropertyName}</c> must be less than or equal to <c>{ComparisonValue}</c>.
    /// </summary>
    public static readonly Error LessThanOrEqual = new(
        "Validation.LessThanOrEqual", "فیلد '{PropertyName}' باید کمتر یا مساوی با {ComparisonValue} باشد.", ErrorType.Validation);
}