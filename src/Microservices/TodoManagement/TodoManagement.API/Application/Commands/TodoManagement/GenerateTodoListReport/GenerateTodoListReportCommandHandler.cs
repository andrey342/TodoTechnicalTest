namespace TodoManagement.API.Application.Commands.TodoManagement.GenerateTodoListReport;

/// <summary>
/// Handles the GenerateTodoListReportCommand by calling PrintItems on the TodoList.
/// The PrintItems method raises a domain event (ItemsPrintedDomainEvent) which is then
/// handled by ItemsPrintedDomainEventHandler to publish the integration event.
/// </summary>
public class GenerateTodoListReportCommandHandler : ICommandHandler<GenerateTodoListReportCommand, bool>
{
    private readonly ITodoListRepository _repository;

    public GenerateTodoListReportCommandHandler(
        ITodoListRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<bool> Handle(GenerateTodoListReportCommand request, CancellationToken cancellationToken)
    {
        var todoList = await _repository.GetByIdAsync(
            request.Id,
            query => query.Include(tl => tl.Items).ThenInclude(i => i.Progressions),
            cancellationToken);

        // Call PrintItems - this will print to console AND raise ItemsPrintedDomainEvent
        todoList.PrintItems();

        // Dispatch domain events
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}