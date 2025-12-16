namespace TodoManagement.API.Apis.Common.Attributes;

/// <summary>
/// Attribute to indicate that a controller or action should be included in the API Gateway.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class IncludeInGatewayAttribute : Attribute
{
    /// <summary>
    /// Text to be displayed as a group in Swagger-UI.
    /// </summary>
    public string? Tag { get; }

    /// <summary>
    /// If <c>true</c>, the Gateway will require authentication for this route.
    /// Default value: <c>false</c>.
    /// </summary>
    public bool RequireAuth { get; }

    /// <summary>
    /// Specifies the Gateway targets for this route.
    /// If <see langword="null"/>, the configured <em>defaultTargets</em> will be used.
    /// </summary>
    public GatewayTarget[] Targets { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IncludeInGatewayAttribute"/> class.
    /// </summary>
    /// <param name="tag">Group name for Swagger-UI.</param>
    /// <param name="requireAuth">Indicates if authentication is required.</param>
    /// <param name="targets">Gateway targets for this route.</param>
    public IncludeInGatewayAttribute(
        string? tag = null,
        bool requireAuth = false,
        params GatewayTarget[] targets
        )
    {
        Tag = tag;
        RequireAuth = requireAuth;
        Targets = targets is { Length: > 0 } ? targets
                                             : GatewayTargetExtensions.GetAllTargets();
    }
}