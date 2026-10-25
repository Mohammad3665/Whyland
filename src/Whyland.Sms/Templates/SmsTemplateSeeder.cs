using Whyland.Sms.Storage;

namespace Whyland.Sms.Templates;

/// <summary>
/// Registers the template the login/OTP flow depends on when the simulator starts, so a
/// fresh process can render OTP messages without manual seeding.
/// </summary>
/// <param name="store">Template store to seed.</param>
/// <param name="timeProvider">UTC clock for the audit stamps.</param>
public sealed class SmsTemplateSeeder(ISmsStore store, TimeProvider timeProvider)
{
    /// <summary>Code of the OTP template used by phone authentication.</summary>
    public const string OtpTemplateCode = "SMS-OTP-001";

    /// <summary>Adds the built-in templates that are not present yet; existing edits are kept.</summary>
    public void Seed()
    {
        if (store.FindTemplate(OtpTemplateCode) is not null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        store.TryAddTemplate(new SmsTemplateRecord
        {
            Code = OtpTemplateCode,
            Name = "تأیید شماره تلفن",
            Description = "کد یک‌بارمصرف ورود/ثبت‌نام (s5.3.1).",
            Content = "کد تأیید شما: {{code}}" + Environment.NewLine
                + "این کد تا {{expirationMinutes}} دقیقه معتبر است.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Parameters = new Dictionary<string, SmsTemplateParameter>(StringComparer.Ordinal)
            {
                ["code"] = new("code", "string", Description: "کد تأیید"),
                ["expirationMinutes"] = new("expirationMinutes", "integer", Description: "مدت اعتبار کد به دقیقه"),
            },
        });
    }
}
