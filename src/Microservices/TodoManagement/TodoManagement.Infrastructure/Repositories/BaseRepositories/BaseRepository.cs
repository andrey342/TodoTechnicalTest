namespace TodoManagement.Infrastructure.Repositories.BaseRepositories;

public class BaseRepository<T> : IBaseRepository<T> where T : Entity
{
    protected readonly TodoManagementContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BaseRepository(TodoManagementContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    /// <summary>
    /// Retrieves an entity by its unique identifier with eager loading of related entities.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="includes">A function to specify related entities to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public async Task<T> GetByIdAsync(
        Guid id,
        Func<IQueryable<T>, IQueryable<T>> includes,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (includes != null)
        {
            query = includes(query);
        }
        
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    /// <summary>
    /// Determines whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity exists; otherwise, false.</returns>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }
}