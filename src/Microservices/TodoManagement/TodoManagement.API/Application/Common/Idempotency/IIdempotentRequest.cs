namespace TodoManagement.API.Application.Common.Idempotency;

/// <summary>
/// Represents a request that supports idempotency by providing a unique identifier.
/// </summary>
public interface IIdempotentRequest
{
    /// <summary>
    /// Gets the unique identifier for the idempotent request.
    /// </summary>
    Guid RequestId { get; }
}