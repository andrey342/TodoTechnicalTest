namespace TodoManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for command operations.
/// Provides default command methods inherited from CommandRepository&lt;T&gt;.
/// </summary>
public class TodoListRepository : CommandRepository<TodoList>, ITodoListRepository
{
    public TodoListRepository(TodoManagementContext context) : base(context) { }
}
