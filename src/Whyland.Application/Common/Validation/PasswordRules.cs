namespace Whyland.Application.Common.Validation;

/// <summary>
/// The single definition of an acceptable local password, shared by registration,
/// password change and password reset so the three flows can never drift apart.
/// </summary>
public static class PasswordRules
{
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
                    .WhylandWithError(ValidationErrors.TooShort, ("Min", minLength))
                // One failure for the whole shape: a password missing both a letter and a digit would
                // otherwise report the same code twice.
                .Must(password => string.IsNullOrEmpty(password) || Satisfies(password, minLength))
                    .WhylandWithError(UserErrors.PasswordTooWeak());
    }
}
