namespace ApiGateway.AG.Extensions;

internal static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers custom services and dependencies for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="env">The web hosting environment.</param>
    /// <param name="logger">The logger instance for the Program class.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, ILogger<Program> logger)
    {
        // Integrate CAP (Event Bus) for distributed transaction and event-driven communication.
        services.AddCapForApiGateway(configuration, env, logger);

        // Register singleton services for route management and Swagger fragment storage.
        services.AddSingleton<RouteStore>();
        services.AddSingleton<SwaggerFragmentStore>();
        services.AddSingleton<DynamicYarpProvider>();

        // Register transient service for gateway route consumption.
        services.AddTransient<GatewayRoutesConsumer>();

        // Return the updated IServiceCollection for chaining.
        return services;
    }
}