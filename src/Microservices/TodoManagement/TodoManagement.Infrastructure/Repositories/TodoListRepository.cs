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
        // Get the TodoList (assuming there is a main one or the first available)
        var todoList = _dbSet
            .OrderBy(tl => tl.Name)
            .FirstOrDefault();

        if (todoList == null)
        {
            // If no TodoList exists, start from 1
            return 1;
        }

        // Increment and update LastIssuedPublicId
        var nextId = todoList.LastIssuedPublicId + 1;
        todoList.UpdateLastIssuedPublicId(nextId);
        
        return nextId;
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