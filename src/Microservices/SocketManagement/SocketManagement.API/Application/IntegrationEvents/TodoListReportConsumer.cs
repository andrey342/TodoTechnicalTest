namespace SocketManagement.API.Application.IntegrationEvents;

/// <summary>
/// Consumes TodoListReportGeneratedIntegrationEvent and sends the file
/// to connected clients via SignalR hub.
/// Handles both raw (small payloads) and compressed (large payloads) events.
/// </summary>
public sealed class TodoListReportConsumer : ICapSubscribe
{
    private readonly IHubContext<PrintHub> _hubContext;
    private readonly ILogger<TodoListReportConsumer> _logger;
    private readonly IEventSerializer _serializer;

    public TodoListReportConsumer(
        IHubContext<PrintHub> hubContext,
        ILogger<TodoListReportConsumer> logger,
        IEventSerializer serializer)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serializer = serializer;
    }

    /// <summary>
    /// Handles small events received as raw objects.
    /// </summary>
    /// <param name="evt">The integration event with report data.</param>
    [CapSubscribe("integration.todolistreportgeneratedintegration.raw")]
    public Task OnRaw(TodoListReportGeneratedIntegrationEvent evt) => ProcessEventAsync(evt);

    /// <summary>
    /// Handles large events received as compressed byte arrays.
    /// </summary>
    /// <param name="data">Compressed event data.</param>
    [CapSubscribe("integration.todolistreportgeneratedintegration.gz")]
    public Task OnGz(byte[] data) 
        => ProcessEventAsync(_serializer.Deserialize<TodoListReportGeneratedIntegrationEvent>(data));

    /// <summary>
    /// Processes the integration event and broadcasts the file to all connected SignalR clients.
    /// </summary>
    /// <param name="evt">The integration event to process.</param>
    private async Task ProcessEventAsync(TodoListReportGeneratedIntegrationEvent evt)
    {
        _logger.LogInformation(
            "Received report for TodoList {TodoListId}: {FileName} ({Size} bytes)",
            evt.TodoListId, evt.FileName, evt.FileContent.Length);

        // Send file info to all connected SignalR clients
        await _hubContext.Clients.All.SendAsync("PrintItems", new
        {
            evt.TodoListId,
            evt.TodoListName,
            evt.FileName,
            evt.ContentType,
            FileContent = Convert.ToBase64String(evt.FileContent)
        });

        _logger.LogInformation(
            "Sent report to SignalR clients for TodoList {TodoListId}",
            evt.TodoListId);
    }
}