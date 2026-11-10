using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Whyland.Domain.Abstractions;

namespace Whyland.Web.Common.ModelBinding;

/// <summary>
/// Binds a wrapped key (<c>CategoryId</c>, <c>BlogCategoryId</c>, <c>UserId</c>, …) from a single
/// submitted value — <c>id=7</c> in a form, a query string or a route segment.
/// <para>
/// Without this the framework treats the identifier struct as a complex object and looks for
/// <c>Id.Value</c>, which no client sends: the property stays at its default and every update
/// command fails its «شناسه باید بزرگ‌تر از 0 باشد» rule. Registered before the numeric binders so
/// it also covers the nullable form (<c>BlogCategoryId?</c>) used for an optional parent.
/// </para>
/// </summary>
public sealed class StronglyTypedIdModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return typeof(IStronglyTypedId).IsAssignableFrom(type)
            ? new StronglyTypedIdModelBinder()
            : null;
    }
}

/// <summary>
/// Reads one submitted value, parses it as the key type the identifier wraps and hands back the
/// wrapped value. Persian and Arabic-Indic digits are converted first, so an id typed — or pasted
/// — with Eastern digits binds like the same id written in Latin ones.
/// </summary>
public sealed class StronglyTypedIdModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) is not null;
        var value = bindingContext.ValueProvider.GetValue(modelName);

        // A missing optional parent is a null, not an error; a missing required key is left unset so
        // the command's own validation reports it.
        if (value == ValueProviderResult.None)
        {
            if (isNullable)
                bindingContext.Result = ModelBindingResult.Success(null);

            return Task.CompletedTask;
        }

        var text = (value.FirstValue ?? string.Empty).Trim();

        if (text.Length == 0)
        {
            if (isNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.ModelState.TryAddModelError(modelName, "شناسه معتبر وارد کنید.");

            return Task.CompletedTask;
        }

        var wrapper = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;
        var raw = StronglyTypedIds.Unwrap(wrapper);

        try
        {
            var key = raw == typeof(Guid)
                ? Guid.Parse(text)
                : Convert.ChangeType(Digits.ToLatinNumber(text), raw, CultureInfo.InvariantCulture);

            var constructor = wrapper.GetConstructor([raw])
                ?? throw new InvalidOperationException(
                    $"Type '{wrapper.Name}' must have a constructor accepting '{raw.Name}'.");

            bindingContext.Result = ModelBindingResult.Success(constructor.Invoke([key]));
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            bindingContext.ModelState.TryAddModelError(modelName, "شناسه معتبر وارد کنید.");
        }

        return Task.CompletedTask;
    }
}