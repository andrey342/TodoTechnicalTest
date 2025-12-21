namespace TodoManagement.API.Application.Commands.TodoManagement.RegisterProgression;

/// <summary>
/// Command to register a new progression entry for a TodoItem.
/// </summary>
public record RegisterProgressionCommand : IdempotentCommand<string>
{
    public Guid TodoListId { get; init; }

    /// <summary>
    /// The business identifier of the TodoItem.
    /// </summary>
    public int ItemId { get; init; }

    /// <summary>
    /// The progression data to register.
    /// </summary>
    public RegisterProgressionDto Progression { get; init; } = new();
}

#region RegisterProgressionDto

/// <summary>
/// Data Transfer Object for progression registration.
/// </summary>
public record RegisterProgressionDto
{
    /// <summary>
    /// The date when the progression was registered.
    /// Must be later than the last progression date.
    /// </summary>
    public DateTime ActionDate { get; init; }

    /// <summary>
    /// The incremental percentage of progress (must be > 0 and not exceed 100% total).
    /// </summary>
    public decimal Percent { get; init; }
}

#endregion