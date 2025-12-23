namespace TodoManagement.API.Application.DomainEventHandlers;

/// <summary>
/// Handles the ItemsPrintedDomainEvent by publishing an integration event
/// with the generated report content.
/// </summary>
public class ItemsPrintedDomainEventHandler : INotificationHandler<ItemsPrintedDomainEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<ItemsPrintedDomainEventHandler> _logger;

    public ItemsPrintedDomainEventHandler(
        IEventBus eventBus,
        ILogger<ItemsPrintedDomainEventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async ValueTask Handle(ItemsPrintedDomainEvent notification, CancellationToken cancellationToken)
    {
        var fileBytes = Encoding.UTF8.GetBytes(notification.Content);
        var fileName = $"{notification.TodoListName}_{notification.OccurredOn:yyyyMMddHHmmss}.txt";

        var integrationEvent = new TodoListReportGeneratedIntegrationEvent(
            TodoListId: notification.TodoListId,
            TodoListName: notification.TodoListName,
            FileContent: fileBytes,
            FileName: fileName
        );

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Published TodoListReportGeneratedIntegrationEvent for TodoList {TodoListId}",
            notification.TodoListId);
    }
}