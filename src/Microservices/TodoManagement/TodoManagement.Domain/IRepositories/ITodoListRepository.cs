namespace TodoManagement.Domain.IRepositories;

/// <summary>
/// Repository interface for command operations.
/// Implements default command methods provided by ICommandRepository&lt;T&gt;.
/// </summary>
public interface ITodoListRepository : ICommandRepository<TodoList>
{
    /// <summary>
    /// Gets the next available identifier for a new TodoItem.
    /// </summary>
    /// <returns>The next available identifier.</returns>
    int GetNextId();

    /// <summary>
    /// Gets all valid categories in the system.
    /// </summary>
    /// <returns>A list of all unique categories.</returns>
    List<string> GetAllCategories();
}