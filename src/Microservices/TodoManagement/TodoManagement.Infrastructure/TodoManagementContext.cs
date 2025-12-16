namespace TodoManagement.Infrastructure;

/// <remarks>
/// Add migrations using the following command inside the 'Ordering.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]
/// </remarks>
public class TodoManagementContext : DbContext, IUnitOfWork
{
    #region DbSets

    #endregion

    private readonly IMediator _mediator;
    private IDbContextTransaction _currentTransaction;

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoManagementContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public TodoManagementContext(DbContextOptions<TodoManagementContext> options) : base(options) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current active database transaction.
    /// </summary>
    /// <returns>The current <see cref="IDbContextTransaction"/> if one exists; otherwise, null.</returns>
    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    /// <summary>
    /// Indicates whether there is an active transaction in progress.
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoManagementContext"/> class with Mediator integration.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    /// <param name="mediator">The mediator instance for publishing domain events.</param>
    public TodoManagementContext(DbContextOptions<TodoManagementContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    #endregion

    #region Model Configuration

    /// <summary>
    /// Configures the model by applying all entity configurations from the current assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoManagementContext).Assembly);
    }

    #endregion

    #region Domain Events

    /// <summary>
    /// Saves all changes made in this context to the database and dispatches domain events.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>True if the changes were successfully saved; otherwise, false.</returns>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before committing data to ensure all side effects are included in the same transaction.
        await DispatchDomainEventsAsync();

        // Commit all changes performed through the DbContext.
        await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Dispatches all domain events associated with tracked entities using Mediator.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity entity && entity.DomainEvents.Any())
            .Select(e => (Entity)e.Entity);

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        domainEntities.ToList().ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }

    #endregion

    #region Transactions

    /// <summary>
    /// Begins a new database transaction if none is currently active.
    /// </summary>
    /// <returns>The newly started <see cref="IDbContextTransaction"/> or null if a transaction is already active.</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    /// <summary>
    /// Commits the specified transaction and saves all changes to the database.
    /// </summary>
    /// <param name="transaction">The transaction to commit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the transaction is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the transaction does not match the current transaction.</exception>
    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException("Transaction mismatch.");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current database transaction if one is active.
    /// </summary>
    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
    #endregion
}