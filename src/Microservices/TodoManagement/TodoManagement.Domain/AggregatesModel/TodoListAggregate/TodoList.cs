namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>Todo list root entity</summary>
/// <remarks>Root of the aggregate; contains the collection of TodoItems</remarks>
public class TodoList : Entity, IAggregateRoot, ITodoList
{
    /// <summary>List name</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>RowVersion for concurrency</summary>
    public byte[] RowVersion { get; private set; } = default!;

    private readonly List<TodoItem> _items = new();
    public IReadOnlyCollection<TodoItem> Items => _items.AsReadOnly();

    private TodoList() { }

    public TodoList(string name)
    {
        ValidateDomainInvariants(name);
        Name = name;
    }

    #region Properties behavior

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    #endregion

    #region ITodoList Implementation

    /// <inheritdoc/>
    public void AddItem(int id, string title, string description, string category)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        ValidateCategory(category);

        var todoItem = new TodoItem(Id, id, title, description, category);
        _items.Add(todoItem);
    }

    /// <inheritdoc/>
    public void UpdateItem(int id, string description)
    {
        ValidateDescription(description);

        var item = FindItemById(id);
        ValidateItemCanBeModified(item, id, "update");

        item.UpdateDescription(description);
    }

    /// <inheritdoc/>
    public void RemoveItem(int id)
    {
        var item = FindItemById(id);
        ValidateItemCanBeModified(item, id, "remove");

        _items.Remove(item);
    }

    /// <inheritdoc/>
    public void RegisterProgression(int id, DateTime dateTime, decimal percent)
    {
        var item = FindItemById(id);
        item.AddProgression(dateTime, percent);
    }

    /// <inheritdoc/>
    public void PrintItems()
    {
        var orderedItems = _items.OrderBy(item => item.ItemId).ToList();

        foreach (var item in orderedItems)
        {
            PrintItem(item);
        }
    }

    private void PrintItem(TodoItem item)
    {
        // Format: {ItemId}) {Title} - {Description} ({Category}) Completed:{IsCompleted}
        Console.Write($"{item.ItemId}) {item.Title} - {item.Description} ({item.Category}) Completed:{item.IsCompleted}");
        Console.WriteLine();

        // Get progressions ordered by date
        var orderedProgressions = item.GetOrderedProgressions().ToList();

        if (orderedProgressions.Any())
        {
            decimal accumulatedProgress = 0m;

            foreach (var progression in orderedProgressions)
            {
                accumulatedProgress += progression.Percent;
                var progressBar = GenerateProgressBar(accumulatedProgress);
                
                // Format: {ActionDate} - {AccumulatedProgress}%     |{ProgressBar}|
                // Date format: M/d/yyyy hh:mm:ss tt (without leading zeros in day/month)
                Console.WriteLine($"{progression.ActionDate:M/d/yyyy hh:mm:ss tt} - {accumulatedProgress}%     |{progressBar}|");
            }
        }
    }

    private string GenerateProgressBar(decimal percentage)
    {
        const int barLength = 50;
        var filledLength = (int)Math.Round(percentage / 100m * barLength);
        filledLength = Math.Min(filledLength, barLength); // Ensure it doesn't exceed the length

        var filled = new string('O', filledLength);
        var empty = new string(' ', barLength - filledLength);

        return filled + empty;
    }

    #endregion

    #region Domain rules

    private const int NameMaxLength = 50;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }
        if (name.Length > NameMaxLength)
        {
            throw new ArgumentException("Name exceeds maximum length of " + NameMaxLength, nameof(name));
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required", nameof(title));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required", nameof(description));
        }
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required", nameof(category));
        }

        if (!Masters.CategoryMaster.IsValidCategory(category))
        {
            var validCategories = string.Join(", ", Masters.CategoryMaster.ValidCategories);
            throw new ArgumentException($"Category '{category}' is invalid. Valid categories are: {validCategories}", nameof(category));
        }
    }

    private TodoItem FindItemById(int id)
    {
        var item = _items.FirstOrDefault(i => i.ItemId == id);
        if (item == null)
        {
            throw new InvalidOperationException($"TodoItem with Id {id} was not found.");
        }
        return item;
    }

    private static void ValidateItemCanBeModified(TodoItem item, int id, string operation)
    {
        if (!item.CanBeModified())
        {
            throw new InvalidOperationException(
                $"Cannot {operation} TodoItem with Id {id} because its total progress ({item.GetTotalProgress()}%) exceeds 50%.");
        }
    }

    private static void ValidateDomainInvariants(string name)
    {
        ValidateName(name);
    }

    #endregion
}