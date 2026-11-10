using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Whyland.Domain.Abstractions;

namespace Whyland.Web.Common.Json;

/// <summary>
/// Reads and writes a wrapped key as the raw value it holds, so a JSON body can carry
/// <c>[{ "id": 7, "displayOrder": 1 }]</c> instead of <c>[{ "id": { "value": 7 }, … }]</c>.
/// <para>
/// This is the body-binding half of <see cref="ModelBinding.StronglyTypedIdModelBinderProvider"/>:
/// form, query and route values go through the binder, <c>[FromBody]</c> payloads go through the
/// input formatter. Without it the reorder endpoint deserializes nothing and its row list arrives
/// null. Writing is covered too, so an identifier returned in a payload is a number or a guid
/// rather than an object the client has to unwrap.
/// </para>
/// </summary>
public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = StronglyTypedIds.Unwrap(typeToConvert);
        var converterType = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, raw);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// Converts one identifier type. The wrapping constructor is compiled once per closed type, the
/// same way the EF value converter does it, because <c>new TId(value)</c> cannot be written against
/// a generic parameter.
/// </summary>
/// <typeparam name="TId">The identifier struct.</typeparam>
/// <typeparam name="TValue">The raw key type it wraps.</typeparam>
public sealed class StronglyTypedIdJsonConverter<TId, TValue> : JsonConverter<TId>
    where TId : struct, IStronglyTypedId<TValue>
{
    private static readonly Func<TValue, TId> Wrap = BuildWrapper();

    /// <inheritdoc />
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Wrap(JsonSerializer.Deserialize<TValue>(ref reader, options)!);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);

    private static Func<TValue, TId> BuildWrapper()
    {
        var constructor = typeof(TId).GetConstructor([typeof(TValue)])
            ?? throw new InvalidOperationException(
                $"Type '{typeof(TId).Name}' must have a constructor accepting '{typeof(TValue).Name}'.");

        var value = Expression.Parameter(typeof(TValue), "value");

        return Expression.Lambda<Func<TValue, TId>>(Expression.New(constructor, value), value).Compile();
    }
}