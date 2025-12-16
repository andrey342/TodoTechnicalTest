namespace TodoManagement.Infrastructure.Repositories.BaseRepositories;

/// <summary>
/// Generic read-only repository implementation for any entity.
/// Should be used exclusively for queries, validation or lookups.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class ValidationOnlyRepository<TEntity> : IValidationOnlyRepository<TEntity>
    where TEntity : Entity
{
    private readonly TodoManagementContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new <see cref="ValidationOnlyRepository{TEntity}"/> instance.
    /// </summary>
    /// <param name="context">EF database context.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ValidationOnlyRepository(TodoManagementContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Efficient Id search using shadow property.
        // For best results, ensure the primary key property is always "Id".
        return await _dbSet.AsNoTracking()
            .AnyAsync(e => EF.Property<Guid>(e, nameof(Entity.Id)) == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        if (filter == null) throw new ArgumentNullException(nameof(filter));
        return await _dbSet.AsNoTracking()
            .AnyAsync(filter, cancellationToken)
            .ConfigureAwait(false);
    }
}