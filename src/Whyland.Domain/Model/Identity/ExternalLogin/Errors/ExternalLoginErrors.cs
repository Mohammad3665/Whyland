using Whyland.Domain.Common.Errors;

using ExternalLoginEntity = Whyland.Domain.Model.Identity.ExternalLogin.Entities.ExternalLogin;

namespace Whyland.Domain.Model.Identity.ExternalLogin.Errors;

/// <summary>
/// Every failure the <see cref="ExternalLoginEntity"/> entity can report.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs; the wording
/// lives in the localization catalog. The prefix is passed by name rather than as a type argument:
/// this row is not an <see cref="IEntity"/>, so <see cref="DomainError.For{TEntity}"/> does not accept it.
/// </para>
/// </summary>
public static class ExternalLoginErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For(nameof(ExternalLogin), nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For(
            nameof(ExternalLogin), nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));
}
