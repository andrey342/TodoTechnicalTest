namespace Contracts.Events.TodoManagement;

public record TodoListReportGeneratedIntegrationEvent(
    Guid TodoListId,
    string TodoListName,
    byte[] FileContent,
    string FileName,
    string ContentType = "text/plain") : IntegrationEvent;