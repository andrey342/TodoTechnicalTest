namespace TodoManagement.Domain.IRepositories.BaseIRepositories;

/// <summary>
/// Generic repository interface that defines basic read operations for an entity type.
/// Provides methods to retrieve an entity by its unique identifier and to check for its existence.
/// </summary>
/// <typeparam name="T">The entity type handled by the repository.</typeparam>
public interface IBaseRepository<T> where T : Entity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>True if the entity exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}