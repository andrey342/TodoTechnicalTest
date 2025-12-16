namespace ApiGateway.AG.Extensions;

/// <summary>
/// Provides extension methods for registering custom middlewares in the application pipeline.
/// </summary>
internal static class MiddlewareExtensions
{
    /// <summary>
    /// Registers custom middlewares to the application's request pipeline.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The application builder with custom middlewares registered.</returns>
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // Registers the ExceptionMiddleware to handle exceptions globally.
        app.UseMiddleware<ExceptionMiddleware>();

        return app;
    }
}