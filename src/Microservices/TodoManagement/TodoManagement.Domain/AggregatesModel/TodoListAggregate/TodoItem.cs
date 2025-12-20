namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>Todo item within the list</summary>
/// <remarks>Stores title, description, category (as string), and holds a progression history; IsCompleted is computed in domain</remarks>
public class TodoItem : Entity
{
    /// <summary>Foreign key to the owning TodoList</summary>
    public Guid TodoListId { get; private set; }

    /// <summary>Business identifier used by the repository (sequential int)</summary>
    public int ItemId { get; private set; }

    /// <summary>Item title</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Item description</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Category as a plain string (validated against system categories via repository)</summary>
    public string Category { get; private set; } = string.Empty;

    private readonly List<Progression> _progressions = new();
    public IReadOnlyCollection<Progression> Progressions => _progressions.AsReadOnly();

    private TodoItem() { }

    public TodoItem(
        Guid todolistid,
        int itemid,
        string title,
        string description,
        string category)
    {
        TodoListId = todolistid;
        ItemId = itemid;
        Title = title;
        Description = description;
        Category = category;
    }

    internal void UpdateTodoListId(Guid todolistid)
    {
        TodoListId = todolistid;
    }

    internal void UpdateItemId(int itemid)
    {
        ItemId = itemid;
    }

    internal void UpdateTitle(string title)
    {
        Title = title;
    }

    internal void UpdateDescription(string description)
    {
        Description = description;
    }

    internal void UpdateCategory(string category)
    {
        Category = category;
    }

    /// <summary>
    /// Gets whether the TodoItem is completed (total progress >= 100%).
    /// </summary>
    internal bool IsCompleted => GetTotalProgress() >= 100m;

    /// <summary>
    /// Gets the total accumulated progress percentage.
    /// </summary>
    internal decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }

    /// <summary>
    /// Checks if the item can be modified (total progress must be <= 50%).
    /// </summary>
    internal bool CanBeModified()
    {
        return GetTotalProgress() <= 50m;
    }

    /// <summary>
    /// Adds a new progression entry with business rule validations.
    /// </summary>
    /// <param name="actionDate">The date when the progression was registered.</param>
    /// <param name="percent">The incremental percentage of progress.</param>
    /// <exception cref="ArgumentException">Thrown when business rules are violated.</exception>
    internal void AddProgression(DateTime actionDate, decimal percent)
    {
        ValidateProgressionPercent(percent);
        ValidateProgressionDate(actionDate);
        ValidateTotalProgress(percent);

        var progression = new Progression(Id, actionDate, percent);
        _progressions.Add(progression);
    }

    /// <summary>
    /// Gets progressions ordered by date.
    /// </summary>
    internal IReadOnlyCollection<Progression> GetOrderedProgressions()
    {
        return _progressions.OrderBy(p => p.ActionDate).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the accumulated progress percentage up to a specific date.
    /// </summary>
    internal decimal GetAccumulatedProgressUpTo(DateTime date)
    {
        return _progressions
            .Where(p => p.ActionDate <= date)
            .OrderBy(p => p.ActionDate)
            .Sum(p => p.Percent);
    }

    #region Domain rules

    internal void ValidateProgressionPercent(decimal percent)
    {
        if (percent <= 0)
        {
            throw new ArgumentException("Percent must be greater than 0.", nameof(percent));
        }
    }

    internal void ValidateProgressionDate(DateTime actionDate)
    {
        if (_progressions.Any())
        {
            var lastProgression = _progressions.OrderByDescending(p => p.ActionDate).First();
            if (actionDate <= lastProgression.ActionDate)
            {
                throw new ArgumentException(
                    $"The new progression date ({actionDate:yyyy-MM-dd}) must be later than the last progression date ({lastProgression.ActionDate:yyyy-MM-dd}).",
                    nameof(actionDate));
            }
        }
    }

    internal void ValidateTotalProgress(decimal percent)
    {
        var currentTotal = GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new ArgumentException(
                $"Cannot add a progression of {percent}% because the total accumulated progress ({currentTotal}%) would exceed 100%.",
                nameof(percent));
        }
    }

    #endregion
}