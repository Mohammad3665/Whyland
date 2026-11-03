using Whyland.Domain.Common.Errors;

using PermissionEntity = Whyland.Domain.Model.Authorization.Permission.Entities.Permission;

namespace Whyland.Domain.Model.Authorization.Permission.Errors;

/// <summary>
/// Every failure the <see cref="PermissionEntity"/> aggregate can report, including the ones raised by the
/// handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs. The wording
/// lives in the localization catalog, so a failure never embeds user input in a hand-written sentence.
/// </para>
/// </summary>
public static class PermissionErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<PermissionEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<PermissionEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));
}
