using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Whyland.Shared.Text;

namespace Whyland.Web.Common.ModelBinding;

/// <summary>
/// Replaces the default numeric binder for the simple number types so a value written with
/// Persian (۰-۹) or Arabic-Indic (٠-٩) digits binds like the same value written in Latin digits.
/// Registered first, so it also covers numbers sent as form, route or query values.
/// </summary>
public sealed class EasternDigitsModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return NumericTypes.Contains(type)
            ? new EasternDigitsModelBinder()
            : null;
    }

    /// <summary>The number types whose text form may be typed with Eastern digits.</summary>
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];
}

/// <summary>
/// Reads one submitted value, converts its digits to Latin and parses it. A value that already
/// uses Latin digits goes through exactly the same parse, so nothing changes for them.
/// </summary>
public sealed class EasternDigitsModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var value = bindingContext.ValueProvider.GetValue(modelName);

        if (value == ValueProviderResult.None)
            return Task.CompletedTask;

        var text = Digits.ToLatinNumber(value.FirstValue).Trim();

        // An empty field means "no value" for a nullable number; a plain one is missing and is
        // reported exactly as the framework would report it, so this binder changes nothing for
        // Latin digits. The empty string is never parsed.
        if (text.Length == 0)
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) is not null)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.ModelState.TryAddModelError(modelName, "مقدار عددی معتبر وارد کنید.");

            return Task.CompletedTask;
        }

        try
        {
            var target = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;
            var number = Convert.ChangeType(text, target, CultureInfo.InvariantCulture);

            bindingContext.Result = ModelBindingResult.Success(number);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            bindingContext.ModelState.TryAddModelError(modelName, "مقدار عددی معتبر وارد کنید.");
        }

        return Task.CompletedTask;
    }
}
