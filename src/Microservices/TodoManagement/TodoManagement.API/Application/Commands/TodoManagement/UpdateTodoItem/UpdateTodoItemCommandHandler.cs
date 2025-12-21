namespace TodoManagement.API.Application.Commands.TodoManagement.UpdateTodoItem;

public class UpdateTodoItemCommandHandler : ICommandHandler<UpdateTodoItemCommand, string>
{
    private readonly TodoListMapper _mapper;
    private readonly ITodoListRepository _repository;

    public UpdateTodoItemCommandHandler(TodoListMapper mapper, ITodoListRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        UpdateTodoItemCommand cmd,
        CancellationToken cancellationToken)
    {
        var todoItem = _mapper.ToEntity(cmd.TodoItem);

        var todoList = await _repository.GetByIdAsync(
            todoItem.TodoListId,
            includes: q => q.Include(tl => tl.Items)
                            .ThenInclude(i => i.Progressions),
            cancellationToken: cancellationToken);

        // Update item (domain validates progress <= 50%)
        todoList.UpdateItem(
            id: todoItem.ItemId,
            description: todoItem.Description);

        await _repository.UpdateAsync(todoList, cancellationToken);

        return todoItem.Id.ToString();
    }
}