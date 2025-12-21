namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate.Masters;

/// <summary>
/// Static class containing the predefined valid categories for TodoItems.
/// </summary>
/// <remarks>
/// DESIGN DECISION: Categories are implemented as a static list rather than a database table.
/// This approach was chosen to focus development time on more critical aspects of the domain
/// (business rules, aggregate behavior, etc.). In a production environment, this could be
/// migrated to a database-backed Categories table for dynamic management.
/// </remarks>
public static class CategoryMaster
{
    /// <summary>
    /// List of all valid categories in the system.
    /// </summary>
    public static IReadOnlyList<string> ValidCategories { get; } = new List<string>
    {
        "Bug Fix",
        "Feature",
        "Refactor",
        "Code Review",
        "Documentation",
        "Testing",
        "DevOps",
        "Research"
    }.AsReadOnly();

    /// <summary>
    /// Checks if the given category is valid.
    /// </summary>
    /// <param name="category">The category to validate.</param>
    /// <returns>True if the category is valid; otherwise, false.</returns>
    public static bool IsValidCategory(string category)
    {
        return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }
}
