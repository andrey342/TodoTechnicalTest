namespace TodoManagement.API.Apis;

public class TodoManagementServices(
    IMediator mediator,
    ITodoListQueries todoListQueries,
    TodoListMapper mapper
    )
{
    public IMediator Mediator { get; } = mediator;
    public ITodoListQueries TodoListQueries { get; set; } = todoListQueries;
    public TodoListMapper Mapper { get; set; } = mapper;
}