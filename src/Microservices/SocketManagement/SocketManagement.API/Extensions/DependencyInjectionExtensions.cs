namespace SocketManagement.API.Extensions;

internal static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers custom services and dependencies for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, ILogger<Program> logger)
    {
        // Integrate CAP (Event Bus) for distributed transaction and event-driven communication.
        services.AddCapForMicroserviceStateless(configuration, env, logger);

        // Register SignalR services for real-time communication
        services.AddSignalR();

        // Mediator is configured to scan for handlers in the assembly containing Program.
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(Program).Assembly];

            // Register Mediator pipeline behaviors in the correct order (outer to inner).
            // LoggingBehavior is the outermost, followed by ValidationBehavior.
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
            ];
        });

        // Register all validators from loaded assemblies for FluentValidation.
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        // Register integration event consumers for CAP
        services.AddTransient<TodoListReportConsumer>();

        // Register the GatewayRoutesPublisher for publishing routes to the event bus.
        services.AddHostedService<GatewayRoutesPublisher>();

        // Return the updated IServiceCollection for chaining.
        return services;
    }
}