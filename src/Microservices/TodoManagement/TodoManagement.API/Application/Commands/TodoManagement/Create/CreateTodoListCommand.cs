namespace TodoManagement.API.Application.Commands.TodoManagement.Create;

/// <summary>
/// Command to create a new TodoList.
/// </summary>
public record CreateTodoListCommand : IdempotentCommand<string>
{
    /// <summary>
    /// The TodoList data to create.
    /// </summary>
    public TodoListDto TodoList { get; init; } = new();
}

#region TodoListDto

/// <summary>
/// Data Transfer Object for TodoList.
/// </summary>
public record TodoListDto
{
    /// <summary>
    /// The name of the TodoList.
    /// </summary>
    public string Name { get; init; } = string.Empty;
}

#endregion