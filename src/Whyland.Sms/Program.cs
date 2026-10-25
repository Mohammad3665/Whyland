using Microsoft.AspNetCore.Authentication;
using Whyland.Sms;
using Whyland.Sms.Application;
using Whyland.Sms.Endpoints;
using Whyland.Sms.Security;
using Whyland.Sms.Storage;
using Whyland.Sms.Templates;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<SmsServiceOptions>()
    .Bind(builder.Configuration.GetSection(SmsServiceOptions.SectionName))
    .Validate(
        options => options.MaxStoredMessages > 0 && options.MaxMessageLength > 0,
        "SmsService:MaxStoredMessages and SmsService:MaxMessageLength must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISmsStore, InMemorySmsStore>();
builder.Services.AddSingleton<SmsTemplateRenderer>();
builder.Services.AddSingleton<SmsTemplateSeeder>();
builder.Services.AddSingleton<SmsSendService>();

// Two separate identities: services present the API key, a person signing into the
// dashboard gets a cookie. Neither policy accepts the other's credential.
builder.Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, SmsApiKeyAuthenticationHandler>(SmsAuthentication.Scheme, _ => { })
    .AddCookie(PanelAuthentication.Scheme, options =>
    {
        options.Cookie.Name = PanelAuthentication.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = PanelAuthentication.SignInPath;
        options.ExpireTimeSpan = PanelAuthentication.SessionLength;
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(
        SmsAuthentication.Policy,
        policy => policy
            .AddAuthenticationSchemes(SmsAuthentication.Scheme)
            .RequireAuthenticatedUser())
    .AddPolicy(
        PanelAuthentication.Policy,
        policy => policy
            .AddAuthenticationSchemes(PanelAuthentication.Scheme)
            .RequireAuthenticatedUser());

// The dashboard (MVC + Razor) is hosted here; its views live in Views/.
builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.Services.GetRequiredService<SmsTemplateSeeder>().Seed();

app.UseExceptionHandler();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapSmsApi();
app.MapOpenApi();

// '/' opens the dashboard rather than a missing Home controller.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
