namespace Whyland.Shared.Errors;

/// <summary>
/// Represents an application error identified by a unique code, an optional description, and an error type category.
/// </summary>
/// <param name="Code">The error code that uniquely identifies the type of error. Must not be null.</param>
/// <param name="Description">An optional description providing additional details about the error.</param>
/// <param name="Type">The category of the error, indicating its nature (e.g., validation, not found, conflict). Defaults to <see cref="ErrorType.None"/>.</param>
/// <param name="Field">The optional name of the input field the error relates to (used for field-level validation messages). Null for non-field errors.</param>
/// <param name="Arguments">Optional formatting arguments for templated error descriptions (e.g., min/max bounds). Null when not needed.</param>
public record Error(string Code, string? Description = null, ErrorType Type = ErrorType.None, string? Field = null, IReadOnlyDictionary<string, object?>? Arguments = null)
{
    /// <summary>
    /// A predefined instance representing no error, with an empty code and <see cref="ErrorType.None"/>.
    /// </summary>
    public static readonly Error None = new Error(string.Empty);

    /// <summary>
    /// Returns the string representation of the error.
    /// If a description is provided, the format is "Code: Description"; otherwise, only the code is returned.
    /// </summary>
    /// <returns>The formatted error string.</returns>
    public override string ToString() =>
        string.IsNullOrEmpty(Description) ? Code : $"{Code}: {Description}";
}
