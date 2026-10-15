using Mapster;

namespace Whyland.Application.Common.Mappings;

/// <summary>
/// Defines a contract for entities or DTOs that require custom object mapping configuration.
/// Implementations provide explicit configuration for object-to-object transformations
/// (custom property mappings, type conversions, conditional transformations).
/// </summary>
public interface IHaveCustomMapping
{
    /// <summary>
    /// Configures custom object mapping rules using Mapster's TypeAdapterConfig.
    /// Implement as a static method on the concrete type; it is discovered and invoked automatically.
    /// </summary>
    /// <param name="config">
    /// The Mapster configuration instance used to define mapping rules.
    /// </param>
    static abstract void ConfigureMapping(TypeAdapterConfig config);
}
