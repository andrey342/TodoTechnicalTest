namespace TodoManagement.Domain.SeedWork.Specifications;

/// <summary>
/// Provides a base implementation for the Specification pattern, encapsulating query logic for entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression that entities must satisfy to be included in the result set.
    /// </summary>
    public Expression<Func<T, bool>> Criteria { get; protected set; }

    /// <summary>
    /// Gets the list of related entities to include in the query result.
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <summary>
    /// Gets the expression used to order the result set.
    /// </summary>
    public Expression<Func<T, object>> OrderBy { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the ordering should be descending.
    /// </summary>
    public bool OrderByDescending { get; protected set; }

    /// <summary>
    /// Gets the number of entities to skip in the result set.
    /// </summary>
    public int? Skip { get; protected set; }

    /// <summary>
    /// Gets the number of entities to take from the result set.
    /// </summary>
    public int? Take { get; protected set; }

    /// <summary>
    /// Sets the criteria expression for filtering entities.
    /// </summary>
    /// <param name="criteria">The criteria expression.</param>
    protected void ApplyCriteria(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;

    /// <summary>
    /// Adds an include expression for related entities to be loaded with the query.
    /// </summary>
    /// <param name="include">The include expression.</param>
    protected void AddInclude(Expression<Func<T, object>> include)
        => Includes.Add(include);

    /// <summary>
    /// Sets the ordering expression and direction for the result set.
    /// </summary>
    /// <param name="orderBy">The ordering expression.</param>
    /// <param name="descending">Indicates whether the ordering should be descending.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy, bool descending = false)
    {
        OrderBy = orderBy;
        OrderByDescending = descending;
    }

    /// <summary>
    /// Sets the paging parameters for the result set.
    /// </summary>
    /// <param name="skip">The number of entities to skip.</param>
    /// <param name="take">The number of entities to take.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }
}