namespace Whyland.Application.Common.Validation;

/// <summary>
/// The single definition of an acceptable local password, shared by registration,
/// password change and password reset so the three flows can never drift apart.
/// </summary>
public static class PasswordRules
{
    /// <summary>Matches at least one Latin or Persian letter.</summary>
    private const string LetterPattern = @"[\p{L}]";

    /// <summary>Matches at least one digit (Latin or Persian).</summary>
    private const string DigitPattern = @"[\p{Nd}]";

    /// <summary>
    /// The same rules as <see cref="WhylandStrongPassword"/>, as a plain predicate, for callers
    /// that must accept an optional value (registration) and cannot use a rule builder there.
    /// </summary>
    /// <param name="password">The candidate password.</param>
    /// <param name="minLength">The configured minimum length.</param>
    public static bool Satisfies(string password, int minLength) =>
        password.Length >= minLength
        && password.Any(char.IsLetter)
        && password.Any(char.IsDigit);

    extension<T>(IRuleBuilder<T, string> rule)
    {
        /// <summary>
        /// Requires at least <paramref name="minLength"/> characters, one letter and one digit.
        /// Empty values are left to the caller's <c>WhylandRequired</c> rule, so an optional
        /// password (registration) only runs these checks when the user actually typed one.
        /// </summary>
        public IRuleBuilderOptions<T, string> WhylandStrongPassword(int minLength)
            => rule
                .MinimumLength(minLength)
                    .WithErrorCode(ValidationErrors.TooShort.Code)
                    .WithMessage($"رمز عبور نمی‌تواند کمتر از {minLength} کاراکتر باشد.")
                .Matches(LetterPattern)
                    .WithErrorCode(ValidationErrors.InvalidFormat.Code)
                    .WithMessage("رمز عبور باید حداقل یک حرف داشته باشد.")
                .Matches(DigitPattern)
                    .WithErrorCode(ValidationErrors.InvalidFormat.Code)
                    .WithMessage("رمز عبور باید حداقل یک رقم داشته باشد.");
    }
}
