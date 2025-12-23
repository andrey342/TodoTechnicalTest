namespace SocketManagement.API.Infrastructure.Services;

/// <summary>
/// Background service responsible for publishing SignalR Hub route configurations during application startup.
/// Unlike REST APIs, SignalR hubs are published as special routes to allow the gateway to be aware of them.
/// </summary>
public sealed class GatewayRoutesPublisher : IHostedService
{
    private readonly ILogger<GatewayRoutesPublisher> _log;
    private readonly IEventBus _bus;
    private readonly IHostApplicationLifetime _life;

    private readonly string _serviceName;
    private readonly string _baseAddress;
    private readonly string _version;

    public GatewayRoutesPublisher(
        IConfiguration cfg,
        ILogger<GatewayRoutesPublisher> log,
        IEventBus bus,
        IHostApplicationLifetime life)
    {
        _log = log;
        _bus = bus;
        _life = life;

        _serviceName = cfg["MS:Name"] ?? throw new("MS:Name missing");
        _baseAddress = cfg["MS:BaseUrl"] ?? throw new("MS:BaseUrl missing");
        _version = typeof(GatewayRoutesPublisher).Assembly.GetName().Version!.ToString();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task PublishAsync()
    {
        // For SignalR hubs, we expose the hub path and the negotiation endpoint.
        // The gateway needs to forward WebSockets and handle the negotiation.
        var hubPath = "/hubs/print";
        
        var evt = new GatewayRoutesEvent(
            Service: _serviceName,
            Version: _version,
            BaseAddress: _baseAddress,
            SwaggerFragmentJson: null,
            Routes: [
                new GatewayRouteDto($"{hubPath}/{{**catch-all}}", "GET", RequireAuth: false),
                new GatewayRouteDto($"{hubPath}/{{**catch-all}}", "POST", RequireAuth: false)
            ]);

        await _bus.PublishAsync(GatewayTargetExtensions.ToConfigString(GatewayTarget.ApiGateway), evt);
        _log.LogInformation("Published SignalR hub routes to ApiGateway: {HubPath}", hubPath);
    }
}