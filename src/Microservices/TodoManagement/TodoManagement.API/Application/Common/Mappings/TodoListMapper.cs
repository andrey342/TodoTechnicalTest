namespace TodoManagement.API.Application.Common.Mappings;

/// <summary>
/// Mapper for TodoList aggregate using Mapperly code generation.
/// </summary>
[Mapper]
public partial class TodoListMapper
{
    #region Dto mappings
    public TodoList ToEntity(TodoListDto dto)
    {
        return new TodoList(
            name: dto.Name,
            lastissuedpublicid: 0);
    }
    #endregion

    #region ViewModel mappings

    public TodoListViewModel ToViewModel(TodoList entity)
    {
        return new TodoListViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            LastIssuedPublicId = entity.LastIssuedPublicId,
            Items = entity.Items.Select(ToViewModel).ToList()
        };
    }

    public TodoItemViewModel ToViewModel(TodoItem entity)
    {
        return new TodoItemViewModel
        {
            Id = entity.Id,
            TodoListId = entity.TodoListId,
            ItemId = entity.ItemId,
            Title = entity.Title,
            Description = entity.Description,
            Category = entity.Category,
            IsCompleted = entity.IsCompleted,
            TotalProgress = entity.GetTotalProgress(),
            Progressions = entity.Progressions.Select(ToViewModel).ToList()
        };
    }

    public partial ProgressionViewModel ToViewModel(Progression entity);

    #endregion
}