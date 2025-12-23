namespace SocketManagement.API.Apis.Common.Swagger;

/// <summary>
/// Swagger operation filter that adds gateway-specific extensions to OpenAPI operations.
/// This filter processes endpoints marked with the IncludeInGatewayAttribute and adds
/// custom extensions to support API Gateway routing configuration.
/// </summary>
public sealed class IncludeInGatewayOperationFilter : IOperationFilter
{
    private readonly string _msName;

    /// <summary>
    /// Initializes a new instance of the IncludeInGatewayOperationFilter class.
    /// </summary>
    /// <param name="cfg">Configuration instance used to retrieve the microservice name.</param>
    /// <exception cref="InvalidOperationException">Thrown when the MS:Name configuration is missing.</exception>
    public IncludeInGatewayOperationFilter(IConfiguration cfg)
    {
        _msName = cfg["MS:Name"] ?? throw new InvalidOperationException("MS:Name missing");
    }

    /// <summary>
    /// Applies gateway-specific extensions to the OpenAPI operation based on the IncludeInGatewayAttribute.
    /// This method adds custom extensions that are used by the API Gateway for routing configuration.
    /// </summary>
    /// <param name="op">The OpenAPI operation to modify.</param>
    /// <param name="ctx">The operation filter context containing metadata about the operation.</param>
    public void Apply(OpenApiOperation op, OperationFilterContext ctx)
    {
        // Check if the endpoint has the IncludeInGatewayAttribute                  
        var attr = ctx.ApiDescription.ActionDescriptor.EndpointMetadata
                       .OfType<IncludeInGatewayAttribute>().FirstOrDefault();
        if (attr is null) return;

        // Mark the operation for inclusion in the gateway
        op.Extensions["x-include-in-gateway"] = new OpenApiBoolean(true);
        
        // Set the gateway tag (use microservice name if no custom tag is specified)
        op.Extensions["x-gateway-tag"] = new OpenApiString(
            string.IsNullOrWhiteSpace(attr.Tag) ? _msName : attr.Tag);

        // Configure authentication requirements for the gateway
        op.Extensions["x-gateway-auth"] = new OpenApiBoolean(attr.RequireAuth);

        // Add gateway target configurations
        var openApiArray = new OpenApiArray();
        foreach (var target in attr.Targets)
            openApiArray.Add(new OpenApiString(target.ToConfigString()));

        op.Extensions["x-gateway-targets"] = openApiArray;
    }
}