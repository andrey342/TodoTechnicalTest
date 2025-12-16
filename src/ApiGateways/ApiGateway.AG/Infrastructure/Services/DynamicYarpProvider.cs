namespace ApiGateway.AG.Infrastructure.Services;

/// <summary>
/// Provides dynamic configuration for YARP proxy based on events from RouteStore.
/// </summary>
public sealed class DynamicYarpProvider : IProxyConfigProvider
{
    #region Fields and Constructor

    private readonly RouteStore _store;
    private readonly IAuthorizationPolicyProvider _policyProvider;

    // Snapshot and cancellation token currently observed by YARP
    private Snapshot _current;
    private CancellationTokenSource _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicYarpProvider"/> class.
    /// </summary>
    /// <param name="store">The route store containing gateway events.</param>
    /// <param name="pp">The authorization policy provider.</param>
    public DynamicYarpProvider(RouteStore store, IAuthorizationPolicyProvider pp)
    {
        _store = store;
        _policyProvider = pp;
        _cts = new CancellationTokenSource();

        _current = new Snapshot(
            Routes: Array.Empty<RouteConfig>(),
            Clusters: Array.Empty<ClusterConfig>(),
            ChangeToken: new CancellationChangeToken(_cts.Token));
    }

    #endregion

    #region IProxyConfigProvider Implementation

    /// <summary>
    /// Gets the current proxy configuration snapshot.
    /// </summary>
    public IProxyConfig GetConfig() => _current;

    #endregion

    #region Configuration Management

    /// <summary>
    /// Rebuilds the proxy configuration when a new event arrives.
    /// </summary>
    public void Rebuild()
    {
        try
        {
            var routes = BuildRoutes();
            var clusters = BuildClusters();

            // 1️ Prepare snapshot with a NEW cancellation token
            var newCts = new CancellationTokenSource();
            var newSnap = new Snapshot(routes, clusters,
                            new CancellationChangeToken(newCts.Token));

            // 2️ Atomic swap of the cancellation token and snapshot
            var oldCts = Interlocked.Exchange(ref _cts, newCts);
            _current = newSnap;

            // 3️ Cancel the old token outside of the YARP thread
            _ = Task.Run(() =>
            {
                oldCts.Cancel();
                oldCts.Dispose();
            });
        }
        catch (Exception ex)
        {
            // Prevents errors from crashing the Gateway
            Console.Error.WriteLine($"[YARP] Error rebuilding config: {ex}");
        }
    }

    /// <summary>
    /// Builds the list of route configurations from the route store.
    /// </summary>
    /// <returns>List of <see cref="RouteConfig"/> objects.</returns>
    private List<RouteConfig> BuildRoutes()
    {
        var list = new List<RouteConfig>();
        // Checks if the 'Authenticated' policy exists
        var hasPolicy = _policyProvider.GetPolicyAsync("Authenticated").Result != null;

        foreach (var evt in _store.All)
        {
            foreach (var r in evt.Routes)
            {
                list.Add(new RouteConfig
                {
                    RouteId = $"{evt.Service}-{r.Method.ToLower()}-{r.Path.Trim('/').Replace('/', '-')}".ToLowerInvariant(),
                    ClusterId = evt.Service,
                    Match = new() { Path = r.Path, Methods = new[] { r.Method } },
                    AuthorizationPolicy = r.RequireAuth && hasPolicy ? "Authenticated" : null,
                });
            }
        }
        return list;
    }

    /// <summary>
    /// Builds the list of cluster configurations from the route store.
    /// </summary>
    /// <returns>List of <see cref="ClusterConfig"/> objects.</returns>
    private List<ClusterConfig> BuildClusters()
    {
        var result = new List<ClusterConfig>();

        foreach (var evt in _store.All)
        {
            result.Add(new ClusterConfig
            {
                ClusterId = evt.Service,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["d1"] = new() { Address = evt.BaseAddress.TrimEnd('/') + "/" }
                }
            });
        }
        return result;
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Private snapshot record implementing <see cref="IProxyConfig"/>.
    /// </summary>
    private sealed record Snapshot(
        IReadOnlyList<RouteConfig> Routes,
        IReadOnlyList<ClusterConfig> Clusters,
        IChangeToken ChangeToken) : IProxyConfig
    {
        IReadOnlyList<RouteConfig> IProxyConfig.Routes => Routes;
        IReadOnlyList<ClusterConfig> IProxyConfig.Clusters => Clusters;
        IChangeToken IProxyConfig.ChangeToken => ChangeToken;
    }

    #endregion
}