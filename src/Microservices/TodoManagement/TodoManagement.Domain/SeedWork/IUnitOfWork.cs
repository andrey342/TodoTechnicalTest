namespace TodoManagement.Domain.SeedWork;

/// <summary>
/// Represents the Unit of Work pattern contract for coordinating the writing of changes
/// and managing transactions within the domain. This interface ensures that all changes
/// to the domain are committed as a single atomic operation.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persists all changes made in the current unit of work to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that returns true if the changes were successfully saved; otherwise, false.
    /// </returns>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}