namespace TodoManagement.API.Application.Queries;

#region TodoList

/// <summary>
/// View model for TodoList entity used in query responses.
/// </summary>
public record TodoListViewModel
{
    /// <summary>
    /// The unique identifier of the TodoList.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The name of the TodoList.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Collection of TodoItems in this TodoList (optional, only included when requested).
    /// </summary>
    public List<TodoItemViewModel> Items { get; init; } = new();
}

#endregion

#region TodoItem

/// <summary>
/// View model for TodoItem entity used in query responses.
/// </summary>
public record TodoItemViewModel
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
    /// Collection of progressions for this TodoItem (optional, only included when requested).
    /// </summary>
    public List<ProgressionViewModel> Progressions { get; init; } = new();
}

#endregion

#region Progression

/// <summary>
/// View model for Progression entity used in query responses.
/// </summary>
public record ProgressionViewModel
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