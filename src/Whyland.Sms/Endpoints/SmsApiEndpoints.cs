using Microsoft.AspNetCore.Http.Extensions;
using Whyland.Sms.Application;
using Whyland.Sms.Templates;

namespace Whyland.Sms.Endpoints;

/// <summary>
/// Maps the versioned v1 SMS contract. Every endpoint requires the API key and returns the
/// provider-style body, so callers never parse HTML or framework error formats (s11.1.2, s11.3.3).
/// </summary>
public static class SmsApiEndpoints
{
    private const string Group = "/api/v1/sms";

    /// <summary>Registers the SMS API routes on the application.</summary>
    public static WebApplication MapSmsApi(this WebApplication app)
    {
        var api = app.MapGroup(Group).RequireAuthorization(SmsAuthentication.Policy);

        api.MapPost("/send", SendAsync);

        api.MapGet("/templates", (ISmsStore store) =>
            Results.Ok(store.ListTemplates().Select(SmsTemplateResponse.From)));

        api.MapGet("/templates/{code}", (string code, ISmsStore store) =>
            store.FindTemplate(code) is { } template
                ? Results.Ok(SmsTemplateResponse.From(template))
                : NotFound(code));

        api.MapPost("/templates", (SaveSmsTemplateRequest request, ISmsStore store, TimeProvider time) =>
            CreateAsync(request, store, time));

        api.MapPut("/templates/{code}", (string code, SaveSmsTemplateRequest request, ISmsStore store, TimeProvider time) =>
            UpdateAsync(code, request, store, time));

        api.MapDelete("/templates/{code}", (string code, ISmsStore store) =>
            store.TryDeleteTemplate(code)
                ? Results.NoContent()
                : NotFound(code));

        api.MapGet("/messages", (string? recipient, string? status, string? templateCode, int? limit, ISmsStore store) =>
            Results.Ok(store.ListMessages(recipient, status, templateCode, limit ?? 100)));

        api.MapGet("/requests", (string? recipient, string? status, int? limit, ISmsStore store) =>
            Results.Ok(store.ListRequests(recipient, status, limit ?? 100)));

        api.MapGet("/requests/{requestId:guid}", (Guid requestId, ISmsStore store) =>
            store.FindRequest(requestId) is { } recorded
                ? Results.Ok(recorded)
                : Results.Json(
                    SendSmsResponse.Failed(SmsErrorCodes.RequestNotFound, $"No request with id '{requestId}'."),
                    statusCode: StatusCodes.Status404NotFound));

        api.MapGet("/{messageId}", (string messageId, ISmsStore store) =>
            store.FindMessage(messageId) is { } message
                ? Results.Ok(message)
                : Results.Json(
                    SendSmsResponse.Failed(SmsErrorCodes.MessageNotFound, $"No message with id '{messageId}'."),
                    statusCode: StatusCodes.Status404NotFound));

        return app;
    }

    private static Task<IResult> SendAsync(
        SendSmsRequest request,
        HttpContext http,
        SmsSendService service) =>
        Task.FromResult(service.Send(request, http.Request.GetEncodedPathAndQuery(), Guid.NewGuid()).ToResult());

    private static Task<IResult> CreateAsync(SaveSmsTemplateRequest request, ISmsStore store, TimeProvider time)
    {
        var code = request.Code?.Trim();

        if (!SmsTemplateEditor.IsAllowedCode(code))
        {
            return Task.FromResult<IResult>(Invalid($"Code must match {SmsTemplateEditor.CodePattern}."));
        }

        var reason = SmsTemplateEditor.Validate(request);
        if (reason is not null)
        {
            return Task.FromResult<IResult>(Invalid(reason));
        }

        var now = time.GetUtcNow().UtcDateTime;
        var template = SmsTemplateEditor.Build(code!, request, now, now);

        if (!store.TryAddTemplate(template))
        {
            return Task.FromResult<IResult>(Results.Json(
                SendSmsResponse.Failed(SmsErrorCodes.InvalidTemplate, $"Template '{code}' already exists."),
                statusCode: StatusCodes.Status409Conflict));
        }

        return Task.FromResult<IResult>(Results.Ok(SmsTemplateResponse.From(template)));
    }

    private static Task<IResult> UpdateAsync(string code, SaveSmsTemplateRequest request, ISmsStore store, TimeProvider time)
    {
        var existing = store.FindTemplate(code);

        if (existing is null)
        {
            return Task.FromResult(NotFound(code));
        }

        var reason = SmsTemplateEditor.Validate(request);
        if (reason is not null)
        {
            return Task.FromResult<IResult>(Invalid(reason));
        }

        var updated = SmsTemplateEditor.Build(code, request, existing.CreatedAt, time.GetUtcNow().UtcDateTime);

        store.TryUpdateTemplate(code, updated);
        return Task.FromResult<IResult>(Results.Ok(SmsTemplateResponse.From(updated)));
    }

    private static IResult Invalid(string reason) =>
        Results.Json(
            SendSmsResponse.Failed(SmsErrorCodes.InvalidTemplate, reason),
            statusCode: StatusCodes.Status400BadRequest);

    private static IResult NotFound(string code) =>
        Results.Json(
            SendSmsResponse.Failed(SmsErrorCodes.TemplateNotFound, $"No template with code '{code}'."),
            statusCode: StatusCodes.Status404NotFound);
}
