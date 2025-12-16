namespace ApiGateway.AG.Extensions;

/// <summary>
/// Provides extension methods for configuring authentication and authorization.
/// </summary>
internal static class AuthenticationExtensions
{
    /// <summary>
    /// Adds generic JWT Bearer authentication and a default authorization policy.
    /// </summary>
    /// <param name="services">The service collection to add authentication to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddGenericAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Retrieve the "Authentication" section from configuration
        var section = configuration.GetSection("Authentication");

        // Configure JWT Bearer authentication
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = section["Authority"];
                options.Audience = section["Audience"];
                options.RequireHttpsMetadata = bool.Parse(section["RequireHttpsMetadata"] ?? "true");
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = section["NameClaimType"] ?? "name",
                    RoleClaimType = section["RoleClaimType"] ?? "roles",
                    // Set clock skew for token validation, defaulting to 120 seconds if not specified
                    ClockSkew = TimeSpan.FromSeconds(int.TryParse(section["ClockSkewSeconds"], out var s) ? s : 120)
                };
            });

        // Add a default authorization policy requiring authenticated users
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }
}