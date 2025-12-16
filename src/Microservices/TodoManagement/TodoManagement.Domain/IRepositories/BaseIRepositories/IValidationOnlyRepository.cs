namespace TodoManagement.Domain.IRepositories.BaseIRepositories;

/// <summary>
/// Generic contract for read-only access to entities.
/// Designed for validation and lookup scenarios only.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IValidationOnlyRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Checks if an entity exists with the given Id.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if found, otherwise false.</returns>
    ValueTask<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the given filter expression.
    /// </summary>
    /// <param name="filter">Filter expression to apply.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if any match is found, otherwise false.</returns>
    ValueTask<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
}