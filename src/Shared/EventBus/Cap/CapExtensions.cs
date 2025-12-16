namespace EventBus.Cap;

/// <summary>
/// Dependency-injection helpers that register:
/// <list type="bullet">
///   <item><description><see cref="JsonEventSerializer"/> as <see cref="IEventSerializer"/></description></item>
///   <item><description><see cref="CapEventBus"/> as <see cref="IEventBus"/> (stateful only)</description></item>
///   <item><description>CAP configured for Kafka with either EF Core or in-memory storage</description></item>
/// </list>
/// <para>
/// Use <see cref="AddCapForMicroserviceStateful{TDbContext}"/> in services that
/// **publish** events and persist outbox/inbox messages with EF Core.
/// </para>
/// <para>
/// Use <see cref="AddCapForMicroserviceStateless"/> in the Stateless microservices (or any
/// consumer-only service) where persistence is not required.
/// </para>
/// </summary>
public static class CapExtensions
{
    #region Microservices Extensions

    /// <summary>
    /// Registers CAP + Kafka using an <see cref="DbContext"/> outbox and adds
    /// the concrete <see cref="CapEventBus"/>.
    /// </summary>
    /// <typeparam name="TDbContext">Application <see cref="DbContext"/>.</typeparam>
    public static IServiceCollection AddCapForMicroserviceStateful<TDbContext>(
        this IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env,
        ILogger logger)
        where TDbContext : DbContext
    {
        // Json serializer (will be reused by CapEventBus).
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();

        // CAP with EF Core outbox.
        ConfigureCap(
            services,
            cfg,
            env,
            logger,
            options => options.UseEntityFramework<TDbContext>());

        // Event-bus abstraction for publishers.
        services.AddSingleton<IEventBus, CapEventBus>();

        return services;
    }

    /// <summary>
    /// Registers CAP + Kafka using **in-memory** storage (no outbox/inbox) for stateless services.
    /// </summary>
    public static IServiceCollection AddCapForMicroserviceStateless(
        this IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env,
        ILogger logger)
    {
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();

        // CAP with volatile storage.
        ConfigureCap(
            services,
            cfg,
            env,
            logger,
            options => options.UseInMemoryStorage());

        // Event-bus abstraction for publishers.
        services.AddSingleton<IEventBus, CapEventBus>();

        return services;
    }

    #endregion

    #region API Gateway Extensions

    /// <summary>
    /// Registers CAP + Kafka using **in-memory** storage (no outbox/inbox) for
    /// stateless services such as a BFF / API-Gateway that only consume events.
    /// </summary>
    public static IServiceCollection AddCapForApiGateway(
        this IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env,
        ILogger logger)
    {
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();

        // CAP with volatile storage.
        ConfigureCap(
            services,
            cfg,
            env,
            logger,
            options => options.UseInMemoryStorage());

        return services;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Centralised CAP configuration shared by both public extension methods.
    /// </summary>
    private static void ConfigureCap(
        IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env,
        ILogger logger,
        Action<CapOptions> storageConfigurer)
    {
        var broker = cfg["Cap:Kafka:Servers"] ??
                     throw new InvalidOperationException("Kafka broker not configured (Cap:Kafka:Servers)");

        services.AddCap(options =>
        {
            storageConfigurer(options); // EF Core or In-Memory
            options.UseKafka(broker);

            if (env.IsDevelopment())
                options.UseDashboard();

            // Retry & retention policies
            options.FailedRetryCount = 5;
            options.SucceedMessageExpiredAfter = 24 * 3600; // 24 h
            options.FailedMessageExpiredAfter = 7 * 24 * 3600; // 7 d
            options.FailedThresholdCallback = failed =>
                logger.LogError("CAP message {Type} failed after retries: {@Payload}",
                                failed.MessageType, failed);
        });

        logger.LogInformation("CAP initialised ({Store}) → Kafka [{Broker}]",
                              storageConfigurer.Method.Name.Contains("InMemory") ? "In-Memory" : "EF Core",
                              broker);
    }

    #endregion
}