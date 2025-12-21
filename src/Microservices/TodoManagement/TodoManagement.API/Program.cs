// Entry point for the TodoManagement API application.
var builder = WebApplication.CreateBuilder(args);
// Retrieve the current hosting environment.
var env = builder.Environment;
// Create a logger instance for the Program class using console logging.
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();

// Register custom infrastructure and configuration services.
builder.Services.AddCustomServices(builder.Configuration, env, logger);
builder.Services.AddCustomSwagger();
builder.Services.AddCustomHealthChecks(builder.Configuration);
builder.Services.AddCustomProblemDetails();

// Register controllers and related services (e.g., API versioning).
builder.Services.AddControllers();

var app = builder.Build();

// Register global middlewares for the application.
app.UseCustomMiddlewares();

// Enable Swagger UI only in the Development environment.
if (app.Environment.IsDevelopment())
    app.UseCustomSwagger();

// Map default controller route for MVC controllers.
app.MapDefaultControllerRoute();

// Register health check endpoints.
app.UseCustomHealthChecks();

// Map the TodoManagement API endpoints.
app.MapTodoManagementApi();

// Start the application.
app.Run();