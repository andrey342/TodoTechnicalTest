namespace TodoManagement.API.Extensions;

/// <summary>
/// Extension methods for handling database migrations and seeding in a hosted service context.
/// </summary>
internal static class MigrateDbContextExtensions
{
    #region Fields

    // Name for the ActivitySource used in tracing database migrations.
    private static readonly string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    #endregion

    #region Public Extension Methods

    /// <summary>
    /// Registers a hosted service to perform database migration without seeding.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddMigration<TContext>((_, _) => Task.CompletedTask);

    /// <summary>
    /// Registers a hosted service to perform database migration and execute a custom seeder.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="seeder">A function to seed the database.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services, Func<TContext, IServiceProvider, Task> seeder)
        where TContext : DbContext
    {
        // Enable migration tracing
        //services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
    }

    /// <summary>
    /// Registers a hosted service to perform database migration and use a specific seeder implementation.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <typeparam name="TDbSeeder">The seeder implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
        return services.AddMigration<TContext>((context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Executes the migration and seeding logic within a scoped service provider.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The root service provider.</param>
    /// <param name="seeder">The seeder function.</param>
    private static async Task MigrateDbContextAsync<TContext>(this IServiceProvider services, Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var scopeServices = scope.ServiceProvider;
        var logger = scopeServices.GetRequiredService<ILogger<TContext>>();
        var context = scopeServices.GetService<TContext>();

        using var activity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(() => InvokeSeeder(seeder, context, scopeServices));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

            activity.SetExceptionTags(ex);

            throw;
        }
    }

    /// <summary>
    /// Ensures the database is created and executes the seeder logic.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="seeder">The seeder function.</param>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="services">The scoped service provider.</param>
    private static async Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            await context.Database.EnsureCreatedAsync();
            await seeder(context, services);
        }
        catch (Exception ex)
        {
            activity.SetExceptionTags(ex);

            throw;
        }
    }

    #endregion

    #region Hosted Service

    /// <summary>
    /// Hosted service that triggers the migration and seeding process on application startup.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    private class MigrationHostedService<TContext>(IServiceProvider serviceProvider, Func<TContext, IServiceProvider, Task> seeder)
        : BackgroundService where TContext : DbContext
    {
        /// <summary>
        /// Starts the migration and seeding process.
        /// </summary>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }

        /// <summary>
        /// No background execution required after startup.
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }

    #endregion
}

#region Seeder Interface

/// <summary>
/// Interface for implementing custom database seeders.
/// </summary>
/// <typeparam name="TContext">The DbContext type.</typeparam>
public interface IDbSeeder<in TContext> where TContext : DbContext
{
    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    Task SeedAsync(TContext context);
}

#endregion