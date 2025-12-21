namespace TodoManagement.API.Application.Commands.TodoManagement.AddTodoItem;

public class AddTodoItemCommandHandler : ICommandHandler<AddTodoItemCommand, string>
{
    private readonly TodoListMapper _mapper;
    private readonly ITodoListRepository _repository;

    public AddTodoItemCommandHandler(TodoListMapper mapper, ITodoListRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        AddTodoItemCommand cmd,
        CancellationToken cancellationToken)
    {
        // Map DTO to Domain Entity
        var todoItem = _mapper.ToEntity(cmd.TodoItem);

        var todoList = await _repository.GetByIdAsync(todoItem.TodoListId, cancellationToken: cancellationToken);

        // Get next available ItemId globally
        var nextItemId = _repository.GetNextId();

        // Add item to the aggregate
        todoList.AddItem(
            id: nextItemId,
            title: todoItem.Title,
            description: todoItem.Description,
            category: todoItem.Category);

        // Update the aggregate in the repository
        await _repository.UpdateAsync(todoList, cancellationToken);

        return todoItem.Id.ToString();
    }
}