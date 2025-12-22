namespace ApiGateway.AG.Extensions;

/// <summary>
/// Extension methods for configuring CORS (Cross-Origin Resource Sharing) policies.
/// </summary>
public static class CorsExtensions
{
    private const string DefaultCorsPolicyName = "DefaultCorsPolicy";

    /// <summary>
    /// Adds CORS services with a default policy that allows requests from the frontend.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Get allowed origins from configuration, or use default for development
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the application to use CORS with the default policy.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
    {
        app.UseCors(DefaultCorsPolicyName);
        return app;
    }
}