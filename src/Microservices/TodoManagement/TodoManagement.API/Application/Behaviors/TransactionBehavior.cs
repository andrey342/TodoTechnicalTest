namespace TodoManagement.API.Application.Behaviors;

// Differences between UnitOfWork and this behavior:
// UnitOfWork: Manages transactions within a single context/repository.
// --------------
// TransactionBehavior: Manages transactions at the application/pipeline level,
// including business logic and coordination between multiple repositories or additional actions.

/// <summary>
/// Pipeline behavior that manages database transactions at the application level.
/// Ensures that each request is executed within a transaction scope, handling commit and rollback as needed.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : ICommand<TResponse>
{
    private readonly TodoManagementContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to manage transactions.</param>
    /// <param name="logger">The logger instance for logging transaction events.</param>
    public TransactionBehavior(TodoManagementContext dbContext, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the request within a transaction scope. If there is no active transaction,
    /// it creates a new one, executes the request, commits the transaction, and publishes integration events.
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
        var response = default(TResponse);
        try
        {
            // If there is already an active transaction, continue without creating a new one.
            if (_dbContext.HasActiveTransaction)
            {
                return await next(request, cancellationToken);
            }

            // Create an execution strategy to handle transient failures.
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                // Begin a new transaction.
                await using var transaction = await _dbContext.BeginTransactionAsync();
                using (_logger.BeginScope(new Dictionary<string, object> { ["TransactionId"] = transaction.TransactionId }))
                {
                    _logger.LogInformation("Beginning transaction {TransactionId} for {Request}", transaction.TransactionId, typeof(TRequest).Name);

                    response = await next(request, cancellationToken);

                    _logger.LogInformation("Committing transaction {TransactionId} for {Request}", transaction.TransactionId, typeof(TRequest).Name);

                    await _dbContext.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            // Log the error and rethrow the exception to ensure proper error handling upstream.
            _logger.LogError(ex, "Error handling transaction for {TransactionId}", request);
            throw;
        }
    }
}