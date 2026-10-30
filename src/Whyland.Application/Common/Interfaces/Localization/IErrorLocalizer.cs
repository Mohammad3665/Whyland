namespace Whyland.Application.Common.Interfaces.Localization;

/// <summary>
/// Turns an error code and its arguments into the text a user is shown.
/// <para>
/// Codes travel through the layers without wording; this is the one place that knows how a sentence is
/// assembled, so a message cannot drift from the rule that produced it and an unknown code cannot leak
/// a raw key into the UI.
/// </para>
/// </summary>
public interface IErrorLocalizer
{
    /// <summary>
    /// Returns a copy of <paramref name="error"/> with <see cref="Error.Description"/> filled in, including
    /// the nested errors an aggregated validation failure carries. Never returns the code as the text.
    /// </summary>
    /// <param name="error">The failure to describe; its code selects the message.</param>
    Error Localize(Error error);
}
