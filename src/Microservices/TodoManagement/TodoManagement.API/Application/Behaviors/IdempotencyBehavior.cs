namespace TodoManagement.API.Application.Behaviors;

/// <summary>
/// Pipeline behavior that enforces idempotency for requests implementing <see cref="IIdempotentRequest"/>.
/// Prevents duplicate processing of requests by checking and recording request identifiers.
/// </summary>
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IRequestManager _requestManager;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="requestManager">The request manager responsible for idempotency checks.</param>
    /// <param name="logger">The logger instance for logging warnings and information.</param>
    public IdempotencyBehavior(
        IRequestManager requestManager,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _requestManager = requestManager;
        _logger = logger;
    }

    /// <summary>
    /// Handles the request, enforcing idempotency if the request implements <see cref="IIdempotentRequest"/>.
    /// If a duplicate request is detected, an exception is thrown.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response from the next handler in the pipeline.</returns>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only enforce idempotency if the request implements IIdempotentRequest
        if (request is not IIdempotentRequest idem)
            return await next(request, cancellationToken);

        // Check if the request with the same identifier already exists
        if (await _requestManager.ExistAsync(idem.RequestId))
        {
            _logger.LogWarning(
                "Duplicate request detected — {RequestId}", idem.RequestId);
            throw new InvalidOperationException("Duplicate request detected.");
        }

        // Record the request to prevent future duplicates
        await _requestManager.CreateRequestForCommandAsync<TRequest>(idem.RequestId);

        return await next(request, cancellationToken);
    }
}