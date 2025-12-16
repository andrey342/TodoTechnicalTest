namespace TodoManagement.API.Infrastructure.Services;

/// <summary>
/// Background service responsible for publishing API Gateway route configurations during application startup.
/// Analyzes Swagger documentation to extract routes marked for gateway inclusion and publishes them
/// to appropriate gateway topics via the event bus.
/// </summary>
public sealed class GatewayRoutesPublisher : IHostedService
{
    #region Fields and Constructor

    private readonly ILogger<GatewayRoutesPublisher> _log;
    private readonly ISwaggerProvider _swagger;
    private readonly IEventBus _bus;
    private readonly IHostApplicationLifetime _life;

    // Configuration values cached for efficiency
    private readonly string _serviceName;
    private readonly string _baseAddress;
    private readonly string _version;

    /// <summary>
    /// Initializes a new instance of the GatewayRoutesPublisher.
    /// </summary>
    /// <param name="cfg">Configuration provider for service metadata</param>
    /// <param name="log">Logger for tracking publishing operations</param>
    /// <param name="swagger">Swagger provider for API documentation</param>
    /// <param name="bus">Event bus for publishing route configurations</param>
    /// <param name="life">Application lifetime for startup coordination</param>
    /// <exception cref="ArgumentException">Thrown when required configuration values are missing</exception>
    public GatewayRoutesPublisher(
        IConfiguration cfg,
        ILogger<GatewayRoutesPublisher> log,
        ISwaggerProvider swagger,
        IEventBus bus,
        IHostApplicationLifetime life)
    {
        _log = log;
        _swagger = swagger;
        _bus = bus;
        _life = life;

        // Cache configuration values and validate required settings
        _serviceName = cfg["MS:Name"] ?? throw new("MS:Name missing");
        _baseAddress = cfg["MS:BaseUrl"] ?? throw new("MS:BaseUrl missing");
        _version = typeof(GatewayRoutesPublisher).Assembly.GetName().Version!.ToString();
    }

    #endregion

    #region IHostedService Implementation
      
    /// <inheritdoc/>
    /// <remarks>
    /// Schedules route publishing to occur after application startup is complete
    /// to ensure all services are properly initialized before route discovery begins.
    /// </remarks>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Schedule publishing to run when the application is fully started
        // This ensures all middleware and services are properly initialized
        _life.ApplicationStarted.Register(static async obj =>
        {
            var self = (GatewayRoutesPublisher)obj!;
            try { await self.PublishAsync().ConfigureAwait(false); }
            catch (Exception ex)
            {
                self._log.LogError(ex, "Error publishing routes during startup");
            }
        }, this);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion

    #region Core Publishing Logic
      
    /// <summary>
    /// Analyzes the Swagger documentation to discover and publish gateway routes.
    /// Processes operations marked with 'x-include-in-gateway' extension and groups them
    /// by target gateway for efficient batch publishing.
    /// </summary>
    /// <returns>A task representing the asynchronous publishing operation</returns>
    private async Task PublishAsync()
    {
        var fullDoc = _swagger.GetSwagger("v1");

        // Step 1: Filter operations that should be included in gateway routing
        // Operations must have the 'x-include-in-gateway' extension set to true
        var ops = fullDoc.Paths
            .SelectMany(p => p.Value.Operations
                .Select(kv => (Path: p.Key, Verb: kv.Key, Op: kv.Value)))
            .Where(t => t.Op.Extensions.TryGetValue("x-include-in-gateway", out var inc) &&
                        inc is OpenApiBoolean { Value: true })
            .ToArray();

        if (ops.Length == 0)
        {
            _log.LogInformation("No operations found for gateway publishing");
            return;
        }

        // Step 2: Group operations by their target gateway destinations
        // Each operation can specify multiple gateway targets via 'x-gateway-targets'
        var groups = ops.SelectMany(t =>
        {
            var targets = (OpenApiArray)t.Op.Extensions["x-gateway-targets"];
            return targets.Select(a => (Gateway: ((OpenApiString)a).Value, t));
        }).GroupBy(x => x.Gateway, StringComparer.OrdinalIgnoreCase);

        // Step 3: Publish route configurations to each gateway in parallel
        // This approach scales well with multiple gateways but avoids overwhelming the message bus
        var tasks = groups.Select(async g =>
        {
            // Create a filtered Swagger fragment containing only relevant operations
            var fragment = BuildFragment(fullDoc, g.Select(x => x.t));

            // Build the gateway routes event with service metadata and route information
            var evt = new GatewayRoutesEvent(
                Service: _serviceName,
                Version: _version,
                BaseAddress: _baseAddress,
                SwaggerFragmentJson: fragment,
                Routes: g.Select(x => new GatewayRouteDto(
                                    x.t.Path,
                                    x.t.Verb.ToString().ToUpperInvariant(),
                                    // Check if authentication is required for this route
                                    x.t.Op.Extensions["x-gateway-auth"] is OpenApiBoolean { Value: true }))
                         .ToList());

            // Publish to the specific gateway topic
            await _bus.PublishAsync(g.Key, evt);
            _log.LogInformation("Published to {Gateway}: {Count} routes", g.Key, evt.Routes.Count);
        });

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Builds a minimal Swagger document fragment containing only the specified operations.
    /// This reduces payload size by including only relevant schemas and endpoints.
    /// </summary>
    /// <param name="src">The source Swagger document</param>
    /// <param name="ops">The operations to include in the fragment</param>
    /// <returns>A JSON string representing the filtered Swagger document</returns>
    private static string BuildFragment(
        OpenApiDocument src,
        IEnumerable<(string Path, OperationType Verb, OpenApiOperation Op)> ops)
    {
        // Create a minimal document with shared schemas but only relevant paths
        var doc = new OpenApiDocument
        {
            Components = new() { Schemas = src.Components.Schemas },
            Paths = new()
        };

        // Add only the operations that are being published
        foreach (var (path, verb, op) in ops)
        {
            if (!doc.Paths.TryGetValue(path, out var item))
                doc.Paths[path] = item = new OpenApiPathItem();
            item.Operations[verb] = op;
        }

        return doc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
    }

    #endregion
}