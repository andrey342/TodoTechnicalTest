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
            lastissuedpublicid: 0
            );
    }

    public TodoItem ToEntity(AddTodoItemDto dto)
    {
        return new TodoItem(
            todolistid: dto.TodoListId,
            itemid: 0,
            title: dto.Title,
            description: dto.Description,
            category: dto.Category
            );
    }

    public TodoItem ToEntity(UpdateTodoItemDto dto)
    {
        return new TodoItem(
            todolistid: dto.TodoListId,
            itemid: dto.ItemId,
            title: string.Empty,
            description: dto.Description,
            category: string.Empty
            );
    }

    public TodoItem ToEntity(RemoveTodoItemDto dto)
    {
        return new TodoItem(
            todolistid: dto.TodoListId,
            itemid: dto.ItemId,
            title: string.Empty,
            description: string.Empty,
            category: string.Empty
            );
    }

    public Progression ToEntity(RegisterProgressionDto dto)
    {
        return new Progression(
            todoitemid: Guid.Empty,
            actiondate: dto.ActionDate,
            percent: dto.Percent
            );
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