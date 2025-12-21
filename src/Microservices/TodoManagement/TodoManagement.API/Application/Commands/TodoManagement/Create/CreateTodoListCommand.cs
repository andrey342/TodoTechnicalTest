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

#region TodoItemDto

/// <summary>
/// Data Transfer Object for TodoItem.
/// </summary>
public record TodoItemDto
{
    /// <summary>
    /// The unique identifier of the TodoItem.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The foreign key to the owning TodoList.
    /// </summary>
    public Guid TodoListId { get; init; }

    /// <summary>
    /// The business identifier used by the repository (sequential int).
    /// </summary>
    public int ItemId { get; init; }

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

    /// <summary>
    /// Indicates whether the TodoItem is completed (total progress >= 100%).
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// The total accumulated progress percentage.
    /// </summary>
    public decimal TotalProgress { get; init; }

    /// <summary>
    /// Collection of progressions for this TodoItem.
    /// </summary>
    public List<ProgressionDto> Progressions { get; init; } = new();
}

#endregion

#region ProgressionDto

/// <summary>
/// Data Transfer Object for Progression.
/// </summary>
public record ProgressionDto
{
    /// <summary>
    /// The unique identifier of the Progression.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The foreign key to the owning TodoItem.
    /// </summary>
    public Guid TodoItemId { get; init; }

    /// <summary>
    /// The date when the progression was registered.
    /// </summary>
    public DateTime ActionDate { get; init; }

    /// <summary>
    /// The incremental percent for this progression entry.
    /// </summary>
    public decimal Percent { get; init; }
}

#endregion