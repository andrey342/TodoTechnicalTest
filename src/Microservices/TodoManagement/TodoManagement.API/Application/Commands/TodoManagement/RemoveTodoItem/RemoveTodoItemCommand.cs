namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoItem;

/// <summary>
/// Command to remove an existing TodoItem from a TodoList.
/// </summary>
public record RemoveTodoItemCommand : ICommand<string>
{
    /// <summary>
    /// The TodoItem removal data.
    /// </summary>
    public RemoveTodoItemDto TodoItem { get; init; } = new();
}

#region RemoveTodoItemDto

/// <summary>
/// Data Transfer Object for TodoItem removal.
/// </summary>
public record RemoveTodoItemDto
{
    /// <summary>
    /// The TodoList identifier that contains the item.
    /// </summary>
    public Guid TodoListId { get; init; }

    /// <summary>
    /// The business identifier of the TodoItem to remove.
    /// </summary>
    public int ItemId { get; init; }
}

#endregion
