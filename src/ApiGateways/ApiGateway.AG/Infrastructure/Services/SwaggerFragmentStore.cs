namespace ApiGateway.AG.Infrastructure.Services;

/// <summary>
/// Stores and manages OpenAPI document fragments for different services.
/// Thread-safe implementation using ConcurrentDictionary.
/// </summary>
public sealed class SwaggerFragmentStore
{
    // Dictionary to hold OpenAPI documents keyed by service name.
    private readonly ConcurrentDictionary<string, OpenApiDocument> _docs = new();

    /// <summary>
    /// Inserts or updates the OpenAPI document for a given service.
    /// </summary>
    /// <param name="svc">The service name.</param>
    /// <param name="doc">The OpenAPI document.</param>
    public void Upsert(string svc, OpenApiDocument doc) => _docs[svc] = doc;

    /// <summary>
    /// Gets all stored OpenAPI documents.
    /// </summary>
    public IEnumerable<OpenApiDocument> All => _docs.Values;
}