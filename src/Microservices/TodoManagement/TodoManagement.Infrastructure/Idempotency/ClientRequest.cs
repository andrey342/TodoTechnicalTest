namespace TodoManagement.Infrastructure.Idempotency;

/// <summary>
/// Represents a client request for idempotency tracking purposes.
/// </summary>
public class ClientRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the client request.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name associated with the client request.
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the client request was made.
    /// </summary>
    public DateTime Time { get; set; }
}