namespace TodoManagement.API.Application.Queries.ProgressionQueries;

/// <summary>
/// Repository implementation for query operations.
/// Provides default query methods inherited from QueryRepository&lt;T&gt;.
/// </summary>
public class ProgressionQueries : QueryRepository<Progression>, IProgressionQueries
{
    public ProgressionQueries(TodoManagementContext context) : base(context) { }
}
