namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>Progress entry for a TodoItem</summary>
/// <remarks>Stores the action date and the incremental percent; domain enforces ascending dates and total <= 100</remarks>
public class Progression : Entity
{
    /// <summary>Foreign key to the owning TodoItem</summary>
    public Guid TodoItemId { get; private set; }
    /// <summary>Date when the progression was registered</summary>
    public DateTime ActionDate { get; private set; }
    /// <summary>Incremental percent for this progression entry (0 < percent < 100)</summary>
    public decimal Percent { get; private set; }
    private Progression() { }

    public Progression(
        Guid todoitemid,
        DateTime actiondate,
        decimal percent)
    {
        TodoItemId = todoitemid;
        ActionDate = actiondate;
        Percent = percent;
    }

    internal void UpdateTodoItemId(Guid todoitemid)
    {
        TodoItemId = todoitemid;
    }

    internal void UpdateActionDate(DateTime actiondate)
    {
        ActionDate = actiondate;
    }

    internal void UpdatePercent(decimal percent)
    {
        Percent = percent;
    }
}
