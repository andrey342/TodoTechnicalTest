namespace ApiGateway.AG.Infrastructure.Services;

/// <summary>
/// Stores and manages gateway route events in a thread-safe manner.
/// </summary>
public sealed class RouteStore
{
    // Thread-safe dictionary to store events by service name.
    private readonly ConcurrentDictionary<string, GatewayRoutesEvent> _events = new();

    /// <summary>
    /// Inserts or updates a gateway route event for a specific service.
    /// </summary>
    /// <param name="e">The gateway route event to upsert.</param>
    public void Upsert(GatewayRoutesEvent e) => _events[e.Service] = e;

    /// <summary>
    /// Gets all stored gateway route events.
    /// </summary>
    public IReadOnlyCollection<GatewayRoutesEvent> All => (IReadOnlyCollection<GatewayRoutesEvent>)_events.Values;
}