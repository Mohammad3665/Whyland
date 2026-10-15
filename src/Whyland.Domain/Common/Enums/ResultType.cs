namespace Whyland.Domain.Common.Enums;

/// <summary>
/// Defines the general type or category of an operation result.
/// Used to describe the overall outcome of an operation when it must be categorized
/// beyond a simple success/failure flag.
/// </summary>
public enum ResultType
{
    /// <summary>
    /// The operation completed successfully without any issues.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The operation encountered an error that prevented completion.
    /// Typically used when an exception or validation failure occurs.
    /// </summary>
    Error = 1,

    /// <summary>
    /// The requested data or entity was not found.
    /// Commonly used when a repository or query returns no matching results.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// The operation completed but raised a non-critical warning.
    /// Often used for partial successes or recoverable states.
    /// </summary>
    Warning = 3
}
