namespace TodoManagement.API.Extensions;

internal static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers custom services and dependencies for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, ILogger<Program> logger)
    {
        // Register the DbContext using the connection string from appsettings (already configured for Docker)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TodoManagementContext>(options =>
            options.UseSqlServer(connectionString));

        // Add the migration extension (without seed, or you can provide your custom seeder)
        services.AddMigration<TodoManagementContext>();

        // Integrate CAP (Event Bus) for distributed transaction and event-driven communication.
        services.AddCapForMicroserviceStateful<TodoManagementContext>(configuration, env, logger);

        // Register infrastructure repositories with scoped lifetime.
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
        services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
        services.AddScoped(typeof(IValidationOnlyRepository<>), typeof(ValidationOnlyRepository<>));

        // Auto-register specific Command Repositories
        services.Scan(scan => scan
            .FromAssemblyOf<TodoManagementContext>()
            .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository") && !c.Name.StartsWith("Base") && !c.Name.StartsWith("Command") && !c.Name.StartsWith("Query") && !c.Name.StartsWith("ValidationOnly")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Auto-register specific Query Repositories
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Queries")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        
        // Register the request manager for handling idempotency.
        services.AddScoped<IRequestManager, RequestManager>();

        // Mediator is configured to scan for handlers in the assembly containing Program.
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(Program).Assembly];

            // Register Mediator pipeline behaviors in the correct order (outer to inner).
            // LoggingBehavior is the outermost, followed by ValidationBehavior, IdempotencyBehavior, then TransactionBehavior.
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(IdempotencyBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(TransactionBehavior<,>)
            ];
        });

        // Register all validators from the application assembly for FluentValidation.
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        
        // Register idempotent command validator as a fallback for commands that implement IIdempotentRequest.
        services.AddTransient(typeof(IValidator<>), typeof(IdempotentCommandValidator<>));

        // Register Mapper files from all loaded assemblies.
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Mapper")))
            .AsSelf()
            .WithSingletonLifetime());

        // Register the CapPublisher for publishing events to the event bus.
        services.AddHostedService<GatewayRoutesPublisher>();

        // Return the updated IServiceCollection for chaining.
        return services;
    }
}