namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoItem;

public class RemoveTodoItemCommandHandler : ICommandHandler<RemoveTodoItemCommand, string>
{
    private readonly TodoListMapper _mapper;
    private readonly ITodoListRepository _repository;

    public RemoveTodoItemCommandHandler(TodoListMapper mapper, ITodoListRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        RemoveTodoItemCommand cmd,
        CancellationToken cancellationToken)
    {
        var todoItem = _mapper.ToEntity(cmd.TodoItem);

        var todoList = await _repository.GetByIdAsync(
            todoItem.TodoListId,
            includes: q => q.Include(tl => tl.Items)
                            .ThenInclude(i => i.Progressions),
            cancellationToken: cancellationToken);

        // Remove item (domain validates progress <= 50%)
        todoList.RemoveItem(todoItem.ItemId);

        await _repository.UpdateAsync(todoList, cancellationToken);

        return todoItem.Id.ToString();
    }
}