namespace TodoManagement.Domain.SeedWork.Specifications;

/// <summary>
/// Defines a contract for a specification pattern, which encapsulates query logic for entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression that entities must satisfy to be included in the result set.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Gets the list of related entities to include in the query result.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the expression used to order the result set.
    /// </summary>
    Expression<Func<T, object>> OrderBy { get; }

    /// <summary>
    /// Gets a value indicating whether the ordering should be descending.
    /// </summary>
    bool OrderByDescending { get; }

    /// <summary>
    /// Gets the number of entities to skip in the result set.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the number of entities to take from the result set.
    /// </summary>
    int? Take { get; }
}