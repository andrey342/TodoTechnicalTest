namespace Contracts.Events;

/// <summary>
/// Data transfer object representing a single gateway route.
/// </summary>
public record GatewayRouteDto(
    string Path,
    string Method,
    bool RequireAuth
    );

/// <summary>
/// Integration event containing information about gateway routes for a specific service.
/// </summary>
public record GatewayRoutesEvent(
    string Service,
    string Version,
    string BaseAddress,
    string SwaggerFragmentJson,
    IReadOnlyList<GatewayRouteDto> Routes)
    : IntegrationEvent;