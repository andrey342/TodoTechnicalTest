namespace TodoManagement.API.Application.Common.Idempotency;

/// <summary>
/// Abstract record representing an idempotent command with a response type.
/// Implements ICommand<TResponse> for Mediator request/response pattern and IIdempotentRequest for idempotency support.
/// <summary>
public abstract record IdempotentCommand<TResponse>()
    : ICommand<TResponse>, IIdempotentRequest
{
    // Unique identifier for the request to ensure idempotency.
    public Guid RequestId { get; init; }
}