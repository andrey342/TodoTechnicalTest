// Entry point for the ApiGateway API application.
var builder = WebApplication.CreateBuilder(args);
// Retrieve the current hosting environment.
var env = builder.Environment;
var isDevelopment = env.IsDevelopment();
// Create a logger instance for the Program class using console logging.
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();

// Register authentication/authorization only outside development environment
if (!isDevelopment)
{
    builder.Services.AddGenericAuthentication(builder.Configuration);
}

// Register Swagger, health checks, and problem details middleware
builder.Services.AddCustomServices(builder.Configuration, env, logger);
builder.Services.AddCustomSwagger();
builder.Services.AddCustomHealthChecks(builder.Configuration);
builder.Services.AddCustomProblemDetails();

// Register YARP reverse proxy services.
// AddReverseProxy registers the core reverse proxy functionality.
builder.Services.AddReverseProxy();

// Register a singleton instance of IProxyConfigProvider.
// This resolves DynamicYarpProvider from the service provider and uses it as the proxy configuration provider.
builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
    sp.GetRequiredService<DynamicYarpProvider>());

builder.Services.AddControllers();

var app = builder.Build();

// Register global exception middleware
app.UseCustomMiddlewares();

// Enable authentication and authorization middleware only outside development
if (!isDevelopment)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Enable Swagger UI only in the development environment
if (isDevelopment)
{
    app.UseCustomSwagger();
}

// Map default controller route for MVC controllers (for custom endpoints, aggregations, etc.)
app.MapDefaultControllerRoute();

// Map the reverse proxy; require auth except in development environment
if (isDevelopment)
{
    app.MapReverseProxy();
}
else
{
    app.MapReverseProxy().RequireAuthorization("Authenticated");
}

// Expose health check endpoint
app.UseCustomHealthChecks();

// Start the application
app.Run();