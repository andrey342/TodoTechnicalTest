namespace ApiGateway.AG.Infrastructure.Swagger;

/// <summary>
/// Document filter for aggregating and normalizing OpenAPI fragments from multiple services.
/// </summary>
public sealed class GatewaySwaggerFilter : IDocumentFilter
{
    private readonly SwaggerFragmentStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewaySwaggerFilter"/> class.
    /// </summary>
    /// <param name="s">The Swagger fragment store.</param>
    public GatewaySwaggerFilter(SwaggerFragmentStore s) => _store = s;

    /// <summary>
    /// Applies the filter to aggregate paths, operations, tags, and schemas from all stored OpenAPI documents.
    /// </summary>
    /// <param name="doc">The target OpenAPI document.</param>
    /// <param name="ctx">The document filter context.</param>
    public void Apply(OpenApiDocument doc, DocumentFilterContext ctx)
    {
        // Iterate through all stored OpenAPI documents
        foreach (var msDoc in _store.All)
        {
            // Merge paths and operations
            foreach (var path in msDoc.Paths)
            {
                // Try to add the path; if it already exists, merge operations
                if (!doc.Paths.TryAdd(path.Key, path.Value))
                    foreach (var kv in path.Value.Operations)
                        doc.Paths[path.Key].Operations.TryAdd(kv.Key, kv.Value);

                // Normalize tags for each operation using the "x-gateway-tag" extension
                foreach (var op in path.Value.Operations.Values)
                {
                    var tag = ((OpenApiString)op.Extensions["x-gateway-tag"]).Value;
                    op.Tags = new List<OpenApiTag> { new() { Name = tag } };
                }
            }

            // Add schemas without duplicating existing ones
            foreach (var sch in msDoc.Components.Schemas)
                doc.Components.Schemas.TryAdd(sch.Key, sch.Value);
        }
    }
}