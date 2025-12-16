namespace ApiGateway.AG.Infrastructure.Services;

/// <summary>
/// Consumes gateway route events and applies them to the internal stores and proxy provider.
/// Handles both raw and compressed event formats.
/// </summary>
public sealed class GatewayRoutesConsumer : ICapSubscribe
{
    private readonly RouteStore _routes;
    private readonly SwaggerFragmentStore _swagger;
    private readonly DynamicYarpProvider _provider;
    private readonly IEventSerializer _ser;
    private readonly ILogger<GatewayRoutesConsumer> _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayRoutesConsumer"/> class.
    /// </summary>
    /// <param name="r">Route store instance.</param>
    /// <param name="s">Swagger fragment store instance.</param>
    /// <param name="p">Dynamic YARP provider instance.</param>
    /// <param name="ser">Event serializer instance.</param>
    /// <param name="log">Logger instance.</param>
    public GatewayRoutesConsumer(RouteStore r, SwaggerFragmentStore s,
                                 DynamicYarpProvider p, IEventSerializer ser,
                                 ILogger<GatewayRoutesConsumer> log)
        => (_routes, _swagger, _provider, _ser, _log) = (r, s, p, ser, log);

    /// <summary>
    /// Handles small events received as objects.
    /// </summary>
    /// <param name="evt">Gateway routes event.</param>
    [CapSubscribe("integration.apigateway.raw")]
    public void OnRaw(GatewayRoutesEvent evt) => Apply(evt);

    /// <summary>
    /// Handles large events received as compressed byte arrays.
    /// </summary>
    /// <param name="data">Compressed event data.</param>
    [CapSubscribe("integration.apigateway.gz")]
    public void OnGz(byte[] data)
        => Apply(_ser.Deserialize<GatewayRoutesEvent>(data));

    /// <summary>
    /// Applies the received event to the route and swagger stores, and triggers proxy configuration rebuild.
    /// </summary>
    /// <param name="evt">Gateway routes event.</param>
    private void Apply(GatewayRoutesEvent evt)
    {
        _log.LogInformation("Applying routes for {Service}", evt.Service);

        _routes.Upsert(evt);
        var reader = new OpenApiStringReader();
        _swagger.Upsert(evt.Service, reader.Read(evt.SwaggerFragmentJson, out _));

        _provider.Rebuild();
    }
}