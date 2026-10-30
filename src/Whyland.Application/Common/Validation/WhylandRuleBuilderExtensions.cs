using System.Linq.Expressions;

namespace Whyland.Application.Common.Validation;

/// <summary>
/// FluentValidation rule-builder extensions that attach Whyland's standardized error codes — and the
/// arguments each message needs — to a rule.
/// <para>
/// A rule states <em>which</em> code failed, never a sentence: <see cref="Behaviors.ValidationBehavior{TRequest,TResponse}"/>
/// turns the failure into an <see cref="Error"/> and the pipeline resolves the wording from the catalog.
/// </para>
/// </summary>
public static class WhylandRuleBuilderExtensions
{
    /// <summary>
    /// Attaches <paramref name="error"/>'s code to the rule and keeps <paramref name="arguments"/> as the
    /// failure's custom state, so the message placeholders can be filled after the code travels back.
    /// </summary>
    private static IRuleBuilderOptions<T, TProperty> WithCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        DomainError error,
        params (string Key, object? Value)[] arguments)
    {
        Dictionary<string, object?> state =
            arguments.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

        return rule.WithErrorCode(error.Code).WithState((_, _) => state);
    }

    /// <summary>
    /// Same as <see cref="WithCode"/>, for a rule whose bound is produced from the object being validated:
    /// the comparison value is only known once the validator runs, so it is read from the model then.
    /// </summary>
    private static IRuleBuilderOptions<T, TProperty> WithCodeFrom<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        DomainError error,
        Func<T, TProperty> comparison)
        => rule.WithErrorCode(error.Code).WithState((model, _) =>
            new Dictionary<string, object?> { ["ComparisonValue"] = comparison(model) });

    /// <summary>
    /// Ensures the property is not empty, using <see cref="ValidationErrors.Required"/>.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> WhylandRequired<T, TProperty>(this IRuleBuilder<T, TProperty> rule)
        => rule.NotEmpty().WithCode(ValidationErrors.Required);

    /// <summary>
    /// String-specific validation rules.
    /// </summary>
    extension<T>(IRuleBuilder<T, string> rule)
    {
        /// <summary>Limits the string to <paramref name="max"/> characters, using <see cref="ValidationErrors.TooLong"/>.</summary>
        /// <param name="max">The maximum allowed length.</param>
        public IRuleBuilderOptions<T, string> WhylandMaxLen(int max)
            => rule.MaximumLength(max).WithCode(ValidationErrors.TooLong, ("Max", max));

        /// <summary>Requires the string to be at least <paramref name="min"/> characters, using <see cref="ValidationErrors.TooShort"/>.</summary>
        /// <param name="min">The minimum allowed length.</param>
        public IRuleBuilderOptions<T, string> WhylandMinLen(int min)
            => rule.MinimumLength(min).WithCode(ValidationErrors.TooShort, ("Min", min));

        /// <summary>Requires the string to be a valid e-mail address, using <see cref="ValidationErrors.InvalidEmail"/>.</summary>
        public IRuleBuilderOptions<T, string> WhylandValidEmail()
            => rule.EmailAddress().WithCode(ValidationErrors.InvalidEmail);

        /// <summary>Requires the string to match <paramref name="pattern"/>, using <see cref="ValidationErrors.InvalidFormat"/>.</summary>
        /// <param name="pattern">The regular expression the string must match.</param>
        public IRuleBuilderOptions<T, string> WhylandMatches(string pattern)
            => rule.Matches(pattern).WithCode(ValidationErrors.InvalidFormat);
    }

    /// <summary>
    /// Comparison-based validation rules for any <see cref="IComparable{T}"/> property.
    /// </summary>
    extension<T, TProperty>(IRuleBuilder<T, TProperty> rule) where TProperty : IComparable<TProperty>, IComparable
    {
        /// <summary>Requires the value to be greater than <paramref name="value"/>, using <see cref="ValidationErrors.GreaterThan"/>.</summary>
        /// <param name="value">The value to compare against.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThan(TProperty value)
            => rule.GreaterThan(value).WithCode(ValidationErrors.GreaterThan, ("ComparisonValue", value));

        /// <summary>Requires the value to be at least <paramref name="value"/>, using <see cref="ValidationErrors.GreaterThanOrEqual"/>.</summary>
        /// <param name="value">The value to compare against.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThanOrEqualTo(TProperty value)
            => rule.GreaterThanOrEqualTo(value).WithCode(ValidationErrors.GreaterThanOrEqual, ("ComparisonValue", value));

        /// <summary>Requires the value to stay below <paramref name="value"/>, using <see cref="ValidationErrors.LessThan"/>.</summary>
        /// <param name="value">The value to compare against.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThan(TProperty value)
            => rule.LessThan(value).WithCode(ValidationErrors.LessThan, ("ComparisonValue", value));

        /// <summary>Requires the value not to exceed <paramref name="value"/>, using <see cref="ValidationErrors.LessThanOrEqual"/>.</summary>
        /// <param name="value">The value to compare against.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThanOrEqualTo(TProperty value)
            => rule.LessThanOrEqualTo(value).WithCode(ValidationErrors.LessThanOrEqual, ("ComparisonValue", value));

        /// <summary>Requires the value to be greater than the one produced by <paramref name="expression"/>, using <see cref="ValidationErrors.GreaterThan"/>.</summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThan(Expression<Func<T, TProperty>> expression)
            => rule.GreaterThan(expression).WithCodeFrom(ValidationErrors.GreaterThan, expression.Compile());

        /// <summary>Requires the value to be at least the one produced by <paramref name="expression"/>, using <see cref="ValidationErrors.GreaterThanOrEqual"/>.</summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandGreaterThanOrEqualTo(Expression<Func<T, TProperty>> expression)
            => rule.GreaterThanOrEqualTo(expression).WithCodeFrom(ValidationErrors.GreaterThanOrEqual, expression.Compile());

        /// <summary>Requires the value to stay below the one produced by <paramref name="expression"/>, using <see cref="ValidationErrors.LessThan"/>.</summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThan(Expression<Func<T, TProperty>> expression)
            => rule.LessThan(expression).WithCodeFrom(ValidationErrors.LessThan, expression.Compile());

        /// <summary>Requires the value not to exceed the one produced by <paramref name="expression"/>, using <see cref="ValidationErrors.LessThanOrEqual"/>.</summary>
        /// <param name="expression">The expression selecting the comparison value.</param>
        public IRuleBuilderOptions<T, TProperty> WhylandLessThanOrEqualTo(Expression<Func<T, TProperty>> expression)
            => rule.LessThanOrEqualTo(expression).WithCodeFrom(ValidationErrors.LessThanOrEqual, expression.Compile());
    }

    /// <summary>
    /// Attaches a code and its arguments to a rule written by hand, for the cases the named helpers above
    /// do not cover (a custom predicate, a rule shared between flows).
    /// </summary>
    /// <param name="error">The error whose code the rule reports.</param>
    /// <param name="arguments">Key/value pairs stored as the failure's custom state.</param>
    public static IRuleBuilderOptions<T, TProperty> WhylandWithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        DomainError error,
        params (string Key, object? Value)[] arguments)
        => rule.WithCode(error, arguments);
}
