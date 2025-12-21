namespace TodoManagement.API.Application.Commands.TodoManagement.Create;

public class CreateTodoListCommandHandler : ICommandHandler<CreateTodoListCommand, string>
{
    private readonly TodoListMapper _mapper;
    private readonly ITodoListRepository _repository;

    public CreateTodoListCommandHandler(TodoListMapper mapper, ITodoListRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        CreateTodoListCommand cmd,
        CancellationToken cancellationToken)
    {
        var todoList = _mapper.ToEntity(cmd.TodoList);

        await _repository.AddAsync(todoList, cancellationToken);

        return todoList.Id.ToString();
    }
}