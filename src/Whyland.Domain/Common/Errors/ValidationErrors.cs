namespace Whyland.Domain.Common.Errors;

/// <summary>
/// The codes FluentValidation rules attach to shape failures (empty, too long, bad format, out of range).
/// <para>
/// Shape errors stay in the validators — that is where the bound value is — but they report one of these
/// codes instead of a sentence, so a client sees the same code whether the rule was checked at the edge
/// or enforced by the aggregate. The numbers a message needs travel as arguments.
/// </para>
/// </summary>
public static class ValidationErrors
{
    /// <summary>Several field failures were aggregated into a single error; the list is in <c>Errors</c>.</summary>
    public static readonly DomainError Failed =
        DomainError.For("Validation", nameof(Failed), ErrorType.Validation);

    /// <summary>The field is required but was empty. Placeholders: <c>{Field}</c>.</summary>
    public static readonly DomainError Required =
        DomainError.For("Validation", nameof(Required), ErrorType.Validation);

    /// <summary>The field is longer than allowed. Placeholders: <c>{Field}</c>, <c>{Max}</c>.</summary>
    public static readonly DomainError TooLong =
        DomainError.For("Validation", nameof(TooLong), ErrorType.Validation);

    /// <summary>The field is shorter than allowed. Placeholders: <c>{Field}</c>, <c>{Min}</c>.</summary>
    public static readonly DomainError TooShort =
        DomainError.For("Validation", nameof(TooShort), ErrorType.Validation);

    /// <summary>The field is not a valid e-mail address. Placeholders: <c>{Field}</c>.</summary>
    public static readonly DomainError InvalidEmail =
        DomainError.For("Validation", nameof(InvalidEmail), ErrorType.Validation);

    /// <summary>The field does not match the format the flow expects. Placeholders: <c>{Field}</c>.</summary>
    public static readonly DomainError InvalidFormat =
        DomainError.For("Validation", nameof(InvalidFormat), ErrorType.Validation);

    /// <summary>The field must exceed a bound. Placeholders: <c>{Field}</c>, <c>{ComparisonValue}</c>.</summary>
    public static readonly DomainError GreaterThan =
        DomainError.For("Validation", nameof(GreaterThan), ErrorType.Validation);

    /// <summary>The field must be at least a bound. Placeholders: <c>{Field}</c>, <c>{ComparisonValue}</c>.</summary>
    public static readonly DomainError GreaterThanOrEqual =
        DomainError.For("Validation", nameof(GreaterThanOrEqual), ErrorType.Validation);

    /// <summary>The field must stay below a bound. Placeholders: <c>{Field}</c>, <c>{ComparisonValue}</c>.</summary>
    public static readonly DomainError LessThan =
        DomainError.For("Validation", nameof(LessThan), ErrorType.Validation);

    /// <summary>The field must not exceed a bound. Placeholders: <c>{Field}</c>, <c>{ComparisonValue}</c>.</summary>
    public static readonly DomainError LessThanOrEqual =
        DomainError.For("Validation", nameof(LessThanOrEqual), ErrorType.Validation);
}
