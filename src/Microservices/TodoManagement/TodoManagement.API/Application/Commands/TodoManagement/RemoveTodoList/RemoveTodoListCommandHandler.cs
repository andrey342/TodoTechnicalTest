namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoList;

public class RemoveTodoListCommandHandler : ICommandHandler<RemoveTodoListCommand, string>
{
    private readonly ITodoListRepository _repository;

    public RemoveTodoListCommandHandler(ITodoListRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<string> Handle(
        RemoveTodoListCommand cmd,
        CancellationToken cancellationToken)
    {
        var todoList = await _repository.GetByIdAsync(
            cmd.Id,
            includes: q => q.Include(tl => tl.Items)
                            .ThenInclude(i => i.Progressions),
            cancellationToken: cancellationToken);

        // Check if all items can be modified (deleted). Rule: Progress <= 50%
        // If there is AT LEAST ONE item that cannot be modified, the domain exception will be thrown.
        todoList.ValidateCanBeDeleted();

        await _repository.DeleteAsync(todoList, cancellationToken);

        return cmd.Id.ToString();
    }
}