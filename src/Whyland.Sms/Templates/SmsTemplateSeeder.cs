namespace Whyland.Sms.Templates;

/// <summary>
/// Registers the templates the authentication flows depend on when the simulator starts, so a
/// fresh process can render OTP messages without manual seeding. Every one-time code — login,
/// phone binding and password recovery — uses the same template; only its purpose differs.
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
        var now = timeProvider.GetUtcNow().UtcDateTime;

        AddIfMissing(OtpTemplateCode, now, new SmsTemplateRecord
        {
            Code = OtpTemplateCode,
            Name = "تأیید شماره تلفن",
            Description = "کد یک‌بارمصرف ورود/ثبت‌نام و بازیابی رمز (s5.3.1, s5.4).",
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

    /// <summary>Inserts one template only when its code is still free, so operator edits survive a restart.</summary>
    private void AddIfMissing(string code, DateTime now, SmsTemplateRecord template)
    {
        if (store.FindTemplate(code) is null)
        {
            store.TryAddTemplate(template);
        }
    }
}
