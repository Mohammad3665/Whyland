using Refit;
using Whyland.Sms.Provider.Contracts;

namespace Whyland.Sms.Provider.Abstractions;

/// <summary>
/// Refit definition of the versioned SMS API. Paths carry the version so an incompatible
/// service release cannot be reached silently (s11.3.10).
/// </summary>
public interface ISmsApiClient
{
    /// <summary>Posts one send request to <c>/api/v1/sms/send</c>.</summary>
    [Post("/api/v1/sms/send")]
    Task<ApiResponse<SmsSendApiResponse>> SendAsync(
        [Body] SmsSendApiRequest request,
        CancellationToken ct);

    /// <summary>Reads one template definition by code.</summary>
    [Get("/api/v1/sms/templates/{code}")]
    Task<ApiResponse<SmsTemplateApiResponse>> GetTemplateAsync(
        string code,
        CancellationToken ct);
}
