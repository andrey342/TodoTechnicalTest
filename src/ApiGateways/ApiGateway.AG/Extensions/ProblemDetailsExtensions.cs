namespace ApiGateway.AG.Extensions;

/// <summary>
/// Extension methods for configuring custom ProblemDetails handling.
/// </summary>
internal static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds and configures custom ProblemDetails middleware to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the ProblemDetails to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        // Register ProblemDetails middleware with custom options if needed.
        services.AddProblemDetails(options =>
        {
            // Custom configuration for ProblemDetails can be added here.
        });
        return services;
    }
}