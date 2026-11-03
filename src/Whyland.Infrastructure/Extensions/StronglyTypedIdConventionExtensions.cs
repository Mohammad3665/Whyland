using Whyland.Domain.Abstractions;
using Whyland.Infrastructure.Persistence.Conventions;

namespace Whyland.Infrastructure.Extensions;

/// <summary>
/// Extension methods that automatically register value converters for every
/// strongly-typed ID discovered in the supplied assemblies.
/// </summary>
public static class StronglyTypedIdConventionExtensions
{
    /// <summary>
    /// Scans the given assemblies and registers a <see cref="ValueConverter{TModel,TProvider}"/>
    /// for every type implementing <see cref="IStronglyTypedId{T}"/>.
    /// </summary>
    /// <param name="configurationBuilder">The EF Core <see cref="ModelConfigurationBuilder"/>.</param>
    /// <param name="assemblies">The assemblies to scan (typically the Domain assembly).</param>
    /// <returns>The same <paramref name="configurationBuilder"/> instance for fluent chaining.</returns>
    public static ModelConfigurationBuilder ConfigureStronglyTypedIds(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        // Discover all types that:
        //   1. Are value types (structs).
        //   2. Are not abstract.
        //   3. Implement IStronglyTypedId<T>.
        var stronglyTypedIdTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsValueType: true, IsAbstract: false })
            .Select(type => new
            {
                Type = type,
                Interface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType 
                        && i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>))
            })
            .Where(x => x.Interface is not null);

        foreach (var item in stronglyTypedIdTypes)
        {
            // Extract TValue from IStronglyTypedId<TValue>.
            var valueType = item.Interface!.GetGenericArguments()[0];

            // Build the closed converter type, e.g. StronglyTypedIdConverter<UserId, Guid>.
            var converterType = typeof(StronglyTypedIdConverter<,>)
                .MakeGenericType(item.Type, valueType);

            // Register the converter for this strongly-typed ID with EF Core.
            configurationBuilder.Properties(item.Type).HaveConversion(converterType);
        }

        return configurationBuilder;
    }
}