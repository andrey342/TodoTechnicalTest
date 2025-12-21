namespace TodoManagement.API.Application.Commands.TodoManagement.RegisterProgression;

public class RegisterProgressionCommandHandler : ICommandHandler<RegisterProgressionCommand, string>
{
    private readonly TodoListMapper _mapper;
    private readonly ITodoListRepository _repository;

    public RegisterProgressionCommandHandler(TodoListMapper mapper, ITodoListRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        RegisterProgressionCommand cmd,
        CancellationToken cancellationToken)
    {
        var progression = _mapper.ToEntity(cmd.Progression);

        var todoList = await _repository.GetByIdAsync(
            cmd.TodoListId,
            includes: q => q.Include(tl => tl.Items)
                            .ThenInclude(i => i.Progressions),
            cancellationToken: cancellationToken);

        // Register progression (domain validates date, percent, and total <= 100%)
        todoList.RegisterProgression(
            id: cmd.ItemId,
            dateTime: progression.ActionDate,
            percent: progression.Percent);

        await _repository.UpdateAsync(todoList, cancellationToken);

        return progression.Id.ToString();
    }
}