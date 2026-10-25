using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Whyland.Sms.Contracts.V1;
using Whyland.Sms.Storage;
using Whyland.Sms.Templates;
using Whyland.Sms.ViewModels;

namespace Whyland.Sms.Controllers;

/// <summary>
/// Template curation for the dashboard. It validates through the same
/// <see cref="SmsTemplateEditor"/> the versioned API uses, so a body that can be saved here is
/// a body the API will accept — and one that cannot is rejected for the same documented reason.
/// </summary>
/// <param name="store">Template store.</param>
/// <param name="renderer">Placeholder engine used by the preview.</param>
/// <param name="time">Clock for the stored timestamps.</param>
public class TemplatesController(
    ISmsStore store,
    SmsTemplateRenderer renderer,
    TimeProvider time
) : PanelController
{
    /// <summary>All templates, ordered by code.</summary>
    public IActionResult Index(string? saved)
    {
        if (saved is not null)
        {
            ViewBag.Saved = saved;
        }

        return View(store.ListTemplates());
    }

    /// <summary>Blank create form.</summary>
    [HttpGet]
    public IActionResult Create() => View("Edit", new TemplateFormViewModel { IsNew = true });

    /// <summary>Stores a new template.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TemplateFormViewModel form)
    {
        ValidateAgainstEditor(form);

        if (!ModelState.IsValid)
        {
            form.IsNew = true;
            return View("Edit", form);
        }

        var now = time.GetUtcNow().UtcDateTime;
        var template = SmsTemplateEditor.Build(form.Code!, form.ToRequest(), now, now);

        if (!store.TryAddTemplate(template))
        {
            ModelState.AddModelError(nameof(form.Code), "این کد قبلاً ثبت شده است؛ یکی دیگر انتخاب کنید.");
            form.IsNew = true;
            return View("Edit", form);
        }

        return RedirectToAction(nameof(Index), new { saved = "created" });
    }

    /// <summary>Loads an existing template into the form.</summary>
    [HttpGet]
    public IActionResult Edit(string code) =>
        store.FindTemplate(code) is { } template
            ? View("Edit", TemplateFormViewModel.From(template))
            : NotFound();

    /// <summary>Replaces an existing template's fields.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string code, TemplateFormViewModel form)
    {
        var existing = store.FindTemplate(code);

        if (existing is null)
        {
            return NotFound();
        }

        ValidateAgainstEditor(form);

        if (!ModelState.IsValid)
        {
            form.IsNew = false;
            form.Code = code;
            return View("Edit", form);
        }

        var updated = SmsTemplateEditor.Build(code, form.ToRequest(), existing.CreatedAt, time.GetUtcNow().UtcDateTime);
        store.TryUpdateTemplate(code, updated);

        return RedirectToAction(nameof(Index), new { saved = "updated" });
    }

    /// <summary>Renders the submitted body with sample values so the curator sees the result.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Preview(TemplateFormViewModel form)
    {
        ValidateAgainstEditor(form);

        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        var now = time.GetUtcNow().UtcDateTime;
        var draft = SmsTemplateEditor.Build(form.Code!, form.ToRequest(), now, now);
        var outcome = renderer.Render(draft, SampleValues(draft));

        ViewBag.Preview = outcome;
        return View("Edit", form);
    }

    /// <summary>Removes a template; sending with it then fails as TemplateNotFound.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string code)
    {
        var removed = store.TryDeleteTemplate(code);
        return RedirectToAction(nameof(Index), new { saved = removed ? "deleted" : "missing" });
    }

    private void ValidateAgainstEditor(TemplateFormViewModel form)
    {
        if (!SmsTemplateEditor.IsAllowedCode(form.Code))
        {
            ModelState.AddModelError(nameof(form.Code), $"کد باید با الگوی {SmsTemplateEditor.CodePattern} بخواند.");
            return;
        }

        var reason = SmsTemplateEditor.Validate(form.ToRequest());

        if (reason is not null)
        {
            ModelState.AddModelError(nameof(form.Content), reason);
        }
    }

    /// <summary>
    /// Placeholder values for the preview: the declared default when it parses as the declared
    /// type, otherwise a sample. Nothing here is ever stored or sent.
    /// </summary>
    private static IReadOnlyDictionary<string, JsonElement> SampleValues(SmsTemplateRecord template)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        foreach (var parameter in template.Parameters.Values)
        {
            var isInteger = parameter.Type.Equals("integer", StringComparison.OrdinalIgnoreCase);
            var candidate = parameter.DefaultValue;

            if (isInteger && int.TryParse(candidate, out var number))
            {
                values[parameter.Name] = JsonSerializer.SerializeToElement(number);
                continue;
            }

            if (!isInteger && !string.IsNullOrWhiteSpace(candidate))
            {
                values[parameter.Name] = JsonSerializer.SerializeToElement(candidate);
                continue;
            }

            values[parameter.Name] = isInteger
                ? JsonSerializer.SerializeToElement(123456)
                : JsonSerializer.SerializeToElement("نمونه");
        }

        return values;
    }
}
