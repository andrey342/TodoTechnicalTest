namespace TodoManagement.Infrastructure.Repositories.BaseRepositories;

public class QueryRepository<T> : BaseRepository<T>, IQueryRepository<T> where T : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public QueryRepository(TodoManagementContext context) : base(context){}

    /// <summary>
    /// Retrieves all entities matching the specified filter and includes, if provided.
    /// </summary>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="includes">Optional function to include related entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of entities.</returns>
    public async Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null)
            query = includes(query);

        if (filter != null)
            query = query.Where(filter);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated and ordered list of entities matching the specified criteria.
    /// </summary>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="orderBy">Expression specifying the property to order by.</param>
    /// <param name="orderByDescending">Indicates if ordering should be descending.</param>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="includes">Optional function to include related entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result containing the entities.</returns>
    public async Task<PaginatedResult<T>> GetPagedAsync(
        int pageIndex, int pageSize,
        Expression<Func<T, object>> orderBy,
        bool orderByDescending = false,
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null)
            query = includes(query);

        if (filter != null)
            query = query.Where(filter);

        var total = await query.CountAsync(cancellationToken);

        query = orderByDescending
            ? query.OrderByDescending(orderBy)
            : query.OrderBy(orderBy);

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(pageIndex, pageSize, total, items);
    }

    /// <summary>
    /// Retrieves the first entity matching the specified filter, or null if no match is found.
    /// </summary>
    /// <param name="filter">Filter expression.</param>
    /// <param name="includes">Optional function to include related entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity, or null.</returns>
    public async Task<T> FirstOrDefaultAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null)
            query = includes(query);

        if (filter != null)
            query = query.Where(filter);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a list of entities that satisfy the given specification.
    /// </summary>
    /// <param name="spec">The specification defining the query criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of entities matching the specification.</returns>
    public async Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Apply criteria
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        // Apply includes
        foreach (var include in spec.Includes)
            query = query.Include(include);

        // Apply ordering
        if (spec.OrderBy != null)
            query = spec.OrderByDescending
                ? query.OrderByDescending(spec.OrderBy)
                : query.OrderBy(spec.OrderBy);

        // Apply paging
        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        return await query.ToListAsync(cancellationToken);
    }
}