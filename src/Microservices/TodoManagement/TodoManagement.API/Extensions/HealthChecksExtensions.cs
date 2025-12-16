namespace TodoManagement.API.Extensions;

/// <summary>
/// Provides extension methods for configuring health checks in the application.
/// </summary>
internal static class HealthChecksExtensions
{
    /// <summary>
    /// Registers health check services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add health checks to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated service collection with health checks registered.</returns>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // Register default health checks, including self, SQL Server, and Kafka.
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "sqlserver")
            .AddKafka(new ProducerConfig
            {
                BootstrapServers = configuration.GetSection("Cap:Kafka:Servers").Value
            }, name: "kafka");
        return services;
    }

    /// <summary>
    /// Configures the application to expose a health check endpoint at "/healthz".
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The updated application builder with health check endpoint configured.</returns>
    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
        // Map the health check endpoint to "/healthz" and customize the response format.
        app.UseHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        component = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        exception = e.Value.Exception?.Message
                    })
                });
                await context.Response.WriteAsync(json);
            }
        });
        return app;
    }
}