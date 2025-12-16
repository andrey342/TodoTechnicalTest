namespace TodoManagement.API.Application.Queries.TodoListQueries;

/// <summary>
/// Repository implementation for query operations.
/// Provides default query methods inherited from QueryRepository&lt;T&gt;.
/// </summary>
public class TodoListQueries : QueryRepository<TodoList>, ITodoListQueries
{
    public TodoListQueries(TodoManagementContext context) : base(context) { }
}
