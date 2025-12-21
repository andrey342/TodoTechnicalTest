namespace TodoManagement.API.Application.Commands.TodoManagement.UpdateTodoItem;

/// <summary>
/// Command to update an existing TodoItem's description.
/// </summary>
public record UpdateTodoItemCommand : ICommand<string>
{
    /// <summary>
    /// The TodoItem update data.
    /// </summary>
    public UpdateTodoItemDto TodoItem { get; init; } = new();
}

#region UpdateTodoItemDto

/// <summary>
/// Data Transfer Object for TodoItem update.
/// </summary>
public record UpdateTodoItemDto
{
    /// <summary>
    /// The TodoList identifier that contains the item.
    /// </summary>
    public Guid TodoListId { get; init; }

    /// <summary>
    /// The business identifier of the TodoItem to update.
    /// </summary>
    public int ItemId { get; init; }

    /// <summary>
    /// The new description for the TodoItem.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

#endregion
