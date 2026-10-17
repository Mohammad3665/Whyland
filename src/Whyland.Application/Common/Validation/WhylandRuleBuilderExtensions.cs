using System.Linq.Expressions;
using Whyland.Domain.Common.Errors;

namespace Whyland.Application.Common.Validation;

/// <summary>
/// FluentValidation rule-builder extensions that attach Whyland's standardized
/// <see cref="Error"/> codes and messages to validation rules.
/// </summary>
public static class WhylandRuleBuilderExtensions
{
    /// <summary>
    /// Applies the code and description of the given <see cref="Error"/> to the rule.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The rule builder to configure.</param>
    /// <param name="error">The error whose code and description are applied.</param>
    /// <returns>The configured rule builder.</returns>
    private static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
        => rule.WithErrorCode(error.Code).WithMessage(error.Description!);

    /// <summary>
    /// Ensures the property is not empty, using <see cref="ValidationErrors.Required"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The rule builder to configure.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, TProperty> WhylandRequired<T, TProperty>(this IRuleBuilder<T, TProperty> rule)
        => rule.NotEmpty().WithErrorCode(ValidationErrors.Required.Code);

    /// <summary>
    /// String-specific validation rules.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="rule">The rule builder to configure.</param>
    extension<T>(IRuleBuilder<T, string> rule)
    {
        /// <summary>
        /// Limits the string to <paramref name="max"/> characters,
        /// using <see cref="ValidationErrors.TooLong"/>.
        /// </summary>
        /// <param name="max">The maximum allowed length.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, string> WhylandMaxLen(int max)
            => rule.MaximumLength(max).WithError(ValidationErrors.TooLong);

        /// <summary>
        /// Requires the string to be at least <paramref name="min"/> characters,
        /// using <see cref="ValidationErrors.TooShort"/>.
        /// </summary>
        /// <param name="min">The minimum allowed length.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, string> WhylandMinLen(int min)
            => rule.MinimumLength(min).WithError(ValidationErrors.TooShort);

        /// <summary>
        /// Requires the string to be a valid email address,
        /// using <see cref="ValidationErrors.InvalidEmail"/>.
        /// </summary>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, string> WhylandValidEmail()
            => rule.EmailAddress().WithError(ValidationErrors.InvalidEmail);

        /// <summary>
        /// Requires the string to match <paramref name="pattern"/>,
        /// using <see cref="ValidationErrors.InvalidFormat"/>.
        /// </summary>
        /// <param name="pattern">The regular expression the string must match.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, string> WhylandMatches(string pattern)
            => rule.Matches(pattern).WithError(ValidationErrors.InvalidFormat);
    }

    /// <summary>
    /// Comparison-based validation rules for any <see cref="IComparable{T}"/> property.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The rule builder to configure.</param>
    extension<T, TProperty>(IRuleBuilder<T, TProperty> rule) where TProperty : IComparable<TProperty>, IComparable
    {
        /// <summary>
        /// Requires the value to be greater than <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThan(TProperty value)
            => rule.GreaterThan(value).WithError(ValidationErrors.GreaterThan);

        /// <summary>
        /// Requires the value to be greater than or equal to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThanOrEqualTo(TProperty value)
            => rule.GreaterThanOrEqualTo(value).WithError(ValidationErrors.GreaterThanOrEqual);

        /// <summary>
        /// Requires the value to be less than <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThan(TProperty value)
            => rule.LessThan(value).WithError(ValidationErrors.LessThan);

        /// <summary>
        /// Requires the value to be less than or equal to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThanOrEqualTo(TProperty value)
            => rule.LessThanOrEqualTo(value).WithError(ValidationErrors.LessThanOrEqual);

        /// <summary>
        /// Requires the value to be greater than the value produced by <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThan(Expression<Func<T, TProperty>> expression)
            => rule.GreaterThan(expression).WithError(ValidationErrors.GreaterThan);

        /// <summary>
        /// Requires the value to be greater than or equal to the value produced by <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThanOrEqualTo(Expression<Func<T, TProperty>> expression)
            => rule.GreaterThanOrEqualTo(expression).WithError(ValidationErrors.GreaterThanOrEqual);

        /// <summary>
        /// Requires the value to be less than the value produced by <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThan(Expression<Func<T, TProperty>> expression)
            => rule.LessThan(expression).WithError(ValidationErrors.LessThan);

        /// <summary>
        /// Requires the value to be less than or equal to the value produced by <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        /// <returns>The configured rule builder.</returns>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThanOrEqualTo(Expression<Func<T, TProperty>> expression)
            => rule.LessThanOrEqualTo(expression).WithError(ValidationErrors.LessThanOrEqual);
    }

    /// <summary>
    /// Attaches a custom error code and a set of arguments to the rule.
    /// The arguments are stored as the validation failure's custom state
    /// (as a dictionary keyed by <c>Key</c>), so they can be surfaced
    /// later by <c>ValidationBehavior</c> when it maps failures to <see cref="Error"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="rule">The rule builder to configure.</param>
    /// <param name="code">The error code to attach.</param>
    /// <param name="arguments">Key/value pairs stored as the failure's custom state.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, TProperty> WhylandWithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, string code,
        params (string Key, object? Value)[] arguments)
        => rule.WithErrorCode(code).WithState(_ => arguments.ToDictionary(x => x.Key, x => x.Value));
}