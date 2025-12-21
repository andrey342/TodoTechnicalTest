namespace TodoManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for command operations.
/// Provides default command methods inherited from CommandRepository&lt;T&gt;.
/// </summary>
public class TodoListRepository : CommandRepository<TodoList>, ITodoListRepository
{
    public TodoListRepository(TodoManagementContext context) : base(context) { }

    /// <inheritdoc/>
    public int GetNextId()
    {
        // Get the maximum ItemId across all TodoItems globally
        var maxItemId = _context.TodoItem
            .OrderByDescending(i => i.ItemId)
            .Select(i => i.ItemId)
            .FirstOrDefault();
        
        return maxItemId + 1;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// DESIGN DECISION: Returns predefined categories from CategoryMaster instead of querying
    /// database. This simplifies the implementation while meeting the interface requirements.
    /// See CategoryMaster.cs for the full list and rationale.
    /// </remarks>
    public List<string> GetAllCategories()
    {
        return CategoryMaster.ValidCategories.ToList();
    }
}