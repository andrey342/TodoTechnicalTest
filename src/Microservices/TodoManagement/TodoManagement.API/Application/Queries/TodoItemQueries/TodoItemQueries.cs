namespace TodoManagement.API.Application.Queries.TodoItemQueries;

/// <summary>
/// Repository implementation for query operations.
/// Provides default query methods inherited from QueryRepository&lt;T&gt;.
/// </summary>
public class TodoItemQueries : QueryRepository<TodoItem>, ITodoItemQueries
{
    public TodoItemQueries(TodoManagementContext context) : base(context) { }
}
