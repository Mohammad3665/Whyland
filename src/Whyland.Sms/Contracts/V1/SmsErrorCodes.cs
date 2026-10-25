namespace Whyland.Sms.Contracts.V1;

/// <summary>
/// Every error code the v1 contract may return. Callers branch on these values, so the
/// strings are part of the published API contract and must not change without a version bump.
/// </summary>
public static class SmsErrorCodes
{
    /// <summary>Header X-Sms-Api-Key is missing or does not match the configured key.</summary>
    public const string Unauthorized = "Sms.Unauthorized";

    /// <summary>The recipient is absent or not a valid phone number.</summary>
    public const string InvalidRecipient = "Sms.InvalidRecipient";

    /// <summary>Neither message text nor a template code was supplied.</summary>
    public const string MissingContent = "Sms.MissingContent";

    /// <summary>Both message text and a template code were supplied; pick one.</summary>
    public const string AmbiguousContent = "Sms.AmbiguousContent";

    /// <summary>The rendered message is empty.</summary>
    public const string EmptyMessage = "Sms.EmptyMessage";

    /// <summary>The rendered message exceeds SmsService:MaxMessageLength.</summary>
    public const string MessageTooLong = "Sms.MessageTooLong";

    /// <summary>No template is registered under the requested code.</summary>
    public const string TemplateNotFound = "Sms.TemplateNotFound";

    /// <summary>The template exists but is inactive.</summary>
    public const string TemplateDisabled = "Sms.TemplateDisabled";

    /// <summary>A required template parameter was not sent.</summary>
    public const string TemplateParameterMissing = "Sms.TemplateParameterMissing";

    /// <summary>A template parameter value does not match the declared type.</summary>
    public const string TemplateParameterInvalid = "Sms.TemplateParameterInvalid";

    /// <summary>The body references a parameter the template does not declare.</summary>
    public const string UnknownTemplateParameter = "Sms.UnknownTemplateParameter";

    /// <summary>Template code is missing, duplicated or otherwise unusable.</summary>
    public const string InvalidTemplate = "Sms.InvalidTemplate";

    /// <summary>No message is stored under the requested message id.</summary>
    public const string MessageNotFound = "Sms.MessageNotFound";

    /// <summary>No inbound request is stored under the requested request id.</summary>
    public const string RequestNotFound = "Sms.RequestNotFound";
}
