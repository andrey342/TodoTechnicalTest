namespace TodoManagement.Infrastructure.Repositories.BaseRepositories;

/// <summary>
/// Generic repository implementation for aggregate roots, providing common data access methods.
/// </summary>
/// <typeparam name="T">The aggregate root type.</typeparam>
public class CommandRepository<T> : BaseRepository<T>, ICommandRepository<T> where T : Entity, IAggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommandRepository(TodoManagementContext context) : base(context){}

    /// <summary>
    /// Gets the unit of work associated with this repository.
    /// </summary>
    public IUnitOfWork UnitOfWork => _context;

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}