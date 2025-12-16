namespace TodoManagement.Infrastructure.Idempotency;

/// <summary>
/// Defines the contract for managing idempotent requests within the system.
/// This interface provides methods to check for the existence of a request
/// and to create a record for a command request, ensuring that duplicate
/// operations are avoided.
/// </summary>
public interface IRequestManager
{
    /// <summary>
    /// Determines whether a request with the specified identifier already exists.
    /// </summary>
    /// <param name="id">The unique identifier of the request.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a boolean value indicating whether the request exists.
    /// </returns>
    Task<bool> ExistAsync(Guid id);

    /// <summary>
    /// Creates a record for a command request with the specified identifier.
    /// This method is used to track processed requests and enforce idempotency.
    /// </summary>
    /// <typeparam name="T">The type of the command associated with the request.</typeparam>
    /// <param name="id">The unique identifier of the request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateRequestForCommandAsync<T>(Guid id);
}