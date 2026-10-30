using Whyland.Application.Common.Behaviors;
using Whyland.Application.Common.Mappings;
using Whyland.Application.Common.Abstractions.CQRS.Dispatchers;

namespace Whyland.Application;

/// <summary>
/// Registers application layer services with dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services including mapping, MediatR, and validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    /// <remarks>
    /// Registers Mapster for object mapping, MediatR with logging pipeline behavior,
    /// and configures FluentValidation for request validation.
    /// </remarks>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMapping();
        // Pipeline behaviors. LoggingBehavior is registered exactly once (outermost)
        // to avoid duplicate log entries; ValidationBehavior then returns validation
        // failures as failure results without reaching the handler.
        // ErrorLocalizationBehavior sits above the validator so a validation failure is described too —
        // ValidationBehavior short-circuits the pipeline, and anything registered below it never runs.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorLocalizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // ValidationBehavior only validates what DI can hand it, so without this registration every
        // command would reach its handler unvalidated.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Facade over MediatR for the outer layers (e.g. Razor Pages).
        services.AddScoped<IDispatcher, MediatRDispatcher>();

        return services;
    }
}
