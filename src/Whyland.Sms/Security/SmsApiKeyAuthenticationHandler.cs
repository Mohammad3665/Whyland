using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Whyland.Sms.Contracts.V1;

namespace Whyland.Sms.Security;

/// <summary>
/// Validates the X-Sms-Api-Key header against SmsService:ApiKey. The comparison is
/// constant-time and the key is never written to logs or responses.
/// <para>
/// With no key configured every request is rejected, so an accidentally unconfigured
/// simulator cannot serve traffic even in development.
/// </para>
/// </summary>
public sealed class SmsApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
    IOptionsMonitor<SmsServiceOptions> settings,
    ILoggerFactory loggerFactory,
    System.Text.Encodings.Web.UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(schemeOptions, loggerFactory, encoder)
{
    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredKey = settings.CurrentValue.ApiKey;

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "SmsService:ApiKey is not configured; set it through user-secrets or the environment."));
        }

        if (!Request.Headers.TryGetValue(SmsAuthentication.HeaderName, out var provided)
            || !FixedTimeEquals(provided.ToString(), configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("The SMS API key is missing or invalid."));
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "sms-client"), new Claim("sms_scheme", Scheme.Name)],
            Scheme.Name,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }

    /// <inheritdoc />
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var body = SendSmsResponse.Failed(
            SmsErrorCodes.Unauthorized,
            $"Requests must carry a valid {SmsAuthentication.HeaderName} header.");

        await JsonSerializer.SerializeAsync(Response.Body, body, JsonOptions);
    }

    private static bool FixedTimeEquals(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    private static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}
