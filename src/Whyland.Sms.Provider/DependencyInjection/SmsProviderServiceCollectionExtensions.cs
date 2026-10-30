using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Whyland.Sms.Provider.Client;

namespace Whyland.Sms.Provider.DependencyInjection;

/// <summary>
/// Registers the SMS service client: options binding, the Refit typed client, and the
/// result-mapping facade (s11.3.8.1).
/// </summary>
public static class SmsProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ISmsApiClient"/> and <see cref="SmsProviderClient"/> using the
    /// configuration section that holds BaseUrl, ApiKey and TimeoutSeconds.
    /// </summary>
    /// <param name="services">Service collection to populate.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name; defaults to "SmsProvider".</param>
    /// <exception cref="InvalidOperationException">Thrown when the base URL is missing or not absolute.</exception>
    public static IServiceCollection AddWhylandSmsProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = SmsProviderOptions.SectionName)
    {
        var section = configuration.GetSection(sectionName);
        services.Configure<SmsProviderOptions>(section);

        var settings = section.Get<SmsProviderOptions>() ?? new SmsProviderOptions();

        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || !Uri.IsWellFormedUriString(settings.BaseUrl, UriKind.Absolute))
        {
            throw new InvalidOperationException(
                $"{sectionName}:BaseUrl must be an absolute URL of the SMS service, e.g. http://localhost:5090.");
        }

        services
            // Generated client: Refit 16 builds the request builders at compile time, so the
            // consumer needs neither runtime reflection nor the Refit.Reflection package.
            .AddRefitGeneratedClient<ISmsApiClient>(
                new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                })
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SmsProviderOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));

                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        SmsProviderOptions.ApiKeyHeaderName,
                        options.ApiKey);
                }
            });

        services.AddSingleton<SmsProviderClient>();

        return services;
    }
}
