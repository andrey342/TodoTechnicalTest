namespace TodoManagement.Domain.IRepositories.BaseIRepositories;

/// <summary>
/// Command repository interface for aggregate roots that provides write operations and unit-of-work support.
/// Exposes methods to add, update and delete aggregate instances and the associated unit of work for transactional operations.
/// </summary>
/// <typeparam name="T">The aggregate root entity type handled by the repository.</typeparam>
public interface ICommandRepository<T> : IBaseRepository<T> where T : Entity, IAggregateRoot
{
    /// <summary>
    /// Gets the unit of work associated with the repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}