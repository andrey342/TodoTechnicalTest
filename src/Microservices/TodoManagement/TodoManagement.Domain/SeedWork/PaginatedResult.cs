namespace TodoManagement.Domain.SeedWork;

/// <summary>
/// Represents a paginated result set for a collection of items of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the items contained in the result set.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Gets the current page index (zero-based).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of items available.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the read-only list of items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResult{T}"/> class.
    /// </summary>
    /// <param name="pageIndex">The current page index (zero-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items available.</param>
    /// <param name="items">The list of items for the current page.</param>
    public PaginatedResult(int pageIndex, int pageSize, int totalCount, IReadOnlyList<T> items)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items;
    }
}