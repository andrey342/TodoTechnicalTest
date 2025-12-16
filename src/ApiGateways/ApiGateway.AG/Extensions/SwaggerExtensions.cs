namespace ApiGateway.AG.Extensions;

internal static class SwaggerExtensions
{
    /// <summary>
    /// Adds and configures Swagger services for API documentation.
    /// </summary>
    /// <param name="services">The IServiceCollection to add Swagger services to.</param>
    /// <returns>The IServiceCollection with Swagger services registered.</returns>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        // Registers the endpoint API explorer required for Swagger.
        services.AddEndpointsApiExplorer();
        // Configures Swagger generation with custom options.
        services.AddSwaggerGen(options =>
        {
            // Defines the Swagger document with title and version.
            options.SwaggerDoc("v1", new() { Title = "ApiGateway AG", Version = "v1" });
            // Applies a custom document filter to modify the generated Swagger document.
            options.DocumentFilter<GatewaySwaggerFilter>();
        });
        return services;
    }

    /// <summary>
    /// Enables Swagger middleware and configures the Swagger UI.
    /// </summary>
    /// <param name="app">The IApplicationBuilder to configure.</param>
    /// <returns>The IApplicationBuilder with Swagger middleware configured.</returns>
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
    {
        // Enables the Swagger middleware to serve the generated Swagger as a JSON endpoint.
        app.UseSwagger();
        // Configures the Swagger UI endpoint and route prefix.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGateway AG V1");
            c.RoutePrefix = "swagger";
        });
        return app;
    }
}