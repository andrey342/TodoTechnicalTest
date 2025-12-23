namespace SocketManagement.API.Application.Hubs;

/// <summary>
/// SignalR Hub for sending print notifications to connected clients.
/// Receives file content from integration events and broadcasts to all clients.
/// </summary>
public class PrintHub : Hub
{
    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message content to send.</param>
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("PrintItems", message);
    }
}