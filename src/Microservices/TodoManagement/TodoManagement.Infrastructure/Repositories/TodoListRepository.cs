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
    public List<string> GetAllCategories()
    {
        // Get all unique categories from existing TodoItems
        var categories = _context.TodoItem
            .Select(item => item.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return categories;
    }
}