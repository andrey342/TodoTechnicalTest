namespace TodoManagement.Domain.IRepositories.BaseIRepositories;

/// <summary>
/// Query repository interface that provides read-only operations for an entity type.
/// Includes methods for retrieving collections, paginated results, single entities and queries based
/// on specifications, with support for filtering and including related entities.
/// </summary>
/// <typeparam name="T">The entity type handled by the query repository.</typeparam>
public interface IQueryRepository<T> : IBaseRepository<T> where T : Entity
{
    /// <summary>
    /// Retrieves all entities matching the specified filter and includes related entities as specified.
    /// </summary>
    /// <param name="filter">An optional filter expression to apply to the query.</param>
    /// <param name="includes">An optional function to include related entities.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>A read-only list of entities matching the criteria.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated and ordered list of entities matching the specified criteria.
    /// </summary>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="orderBy">An expression specifying the property to order by.</param>
    /// <param name="orderByDescending">Indicates whether the ordering should be descending.</param>
    /// <param name="filter">An optional filter expression to apply to the query.</param>
    /// <param name="includes">An optional function to include related entities.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>A paginated result containing the entities.</returns>
    Task<PaginatedResult<T>> GetPagedAsync(
        int pageIndex, int pageSize,
        Expression<Func<T, object>> orderBy,
        bool orderByDescending = false,
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity matching the specified filter, or null if no match is found.
    /// </summary>
    /// <param name="filter">A filter expression to apply to the query.</param>
    /// <param name="includes">An optional function to include related entities.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>The first matching entity, or null.</returns>
    Task<T> FirstOrDefaultAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>> includes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of entities that satisfy the given specification.
    /// </summary>
    /// <param name="spec">The specification defining the query criteria.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>A read-only list of entities matching the specification.</returns>
    Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default);
}