namespace TodoManagement.API.Application.Commands.TodoManagement.AddTodoItem;

/// <summary>
/// Command to add a new TodoItem to an existing TodoList.
/// </summary>
public record AddTodoItemCommand : IdempotentCommand<string>
{
    /// <summary>
    /// The TodoItem data to create.
    /// </summary>
    public AddTodoItemDto TodoItem { get; init; } = new();
}

#region TodoItemDto

/// <summary>
/// Data Transfer Object for TodoItem creation.
/// </summary>
public record AddTodoItemDto
{
    /// <summary>
    /// The TodoList identifier to add the item to.
    /// </summary>
    public Guid TodoListId { get; init; }

    /// <summary>
    /// The title of the TodoItem.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The description of the TodoItem.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The category of the TodoItem.
    /// </summary>
    public string Category { get; init; } = string.Empty;
}

#endregion