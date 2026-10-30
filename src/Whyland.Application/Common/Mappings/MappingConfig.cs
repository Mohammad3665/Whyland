namespace Whyland.Application.Common.Mappings;

/// <summary>
/// Configures Mapster mappings for the application.
/// Automatically registers mappings for types implementing <see cref="IMapFrom{TSource}"/> or
/// <see cref="IMapTo{TDestination}"/>.
/// Supports custom mapping configuration via <see cref="IHaveCustomMapping"/>.
/// </summary>
public class MappingConfig : IRegister
{
    /// <summary>
    /// Registers all mapping configurations.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        var assembly = typeof(MappingConfig).Assembly;

        var mapFromTypes = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();

        foreach (var type in mapFromTypes)
        {
            var source = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>))
                .GetGenericArguments()[0];

            if (typeof(IHaveCustomMapping).IsAssignableFrom(type))
            {
                var method = type.GetMethod(nameof(IHaveCustomMapping.ConfigureMapping),
                    BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [config]);
            }
            else
                config.NewConfig(source, type);
        }

        var mapToTypes = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapTo<>)))
            .ToList();

        foreach (var type in mapToTypes)
        {
            var destination = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapTo<>))
                .GetGenericArguments()[0];

            if (typeof(IHaveCustomMapping).IsAssignableFrom(type))
            {
                var method = type.GetMethod(nameof(IHaveCustomMapping.ConfigureMapping),
                    BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, [config]);
            }
            else
                config.NewConfig(type, destination);
        }
    }
}
