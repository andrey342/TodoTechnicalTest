namespace TodoManagement.Infrastructure.Idempotency;

/// <summary>
/// Manages idempotent requests to ensure that duplicate operations are not processed.
/// </summary>
public class RequestManager : IRequestManager
{
    private readonly TodoManagementContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestManager"/> class.
    /// </summary>
    /// <param name="context">The database context used for request tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
    public RequestManager(TodoManagementContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Determines whether a client request with the specified identifier exists in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the client request.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a boolean value indicating whether the request exists.
    /// </returns>
    public async Task<bool> ExistAsync(Guid id)
    {
        var request = await _context.FindAsync<ClientRequest>(id);
        return request != null;
    }

    /// <summary>
    /// Creates a new client request record for the specified command type and identifier.
    /// Throws an exception if a request with the same identifier already exists.
    /// </summary>
    /// <typeparam name="T">The type of the command associated with the request.</typeparam>
    /// <param name="id">The unique identifier of the client request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown if a request with the specified identifier already exists.</exception>
    public async Task CreateRequestForCommandAsync<T>(Guid id)
    {
        var exists = await ExistAsync(id);

        var request = exists ?
            throw new Exception($"Request with {id} already exists") :
            new ClientRequest()
            {
                Id = id,
                Name = typeof(T).Name,
                Time = DateTime.UtcNow
            };

        _context.Add(request);

        await _context.SaveChangesAsync();
    }
}