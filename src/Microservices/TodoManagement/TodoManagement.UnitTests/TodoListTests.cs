using FluentAssertions;
using TodoManagement.Domain.AggregatesModel.TodoListAggregate;
using Xunit;

namespace TodoManagement.UnitTests;

public class TodoListTests
{
    private readonly TodoList _todoList;
    private const int DefaultItemId = 1;

    public TodoListTests()
    {
        _todoList = new TodoList("Test List");
    }

    [Fact]
    public void AddItem_Should_Succeed_When_CategoryIsValid()
    {
        // Arrange
        var category = "Work"; // Valid category

        // Act
        _todoList.AddItem(DefaultItemId, "Title", "Desc", category);

        // Assert
        _todoList.Items.Should().ContainSingle(i => i.Category == category);
    }

    [Fact]
    public void AddItem_Should_Throw_When_CategoryIsInvalid()
    {
        // Arrange
        var category = "InvalidCategory";

        // Act
        Action act = () => _todoList.AddItem(DefaultItemId, "Title", "Desc", category);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Category 'InvalidCategory' is invalid.*");
    }

    [Fact]
    public void RegisterProgression_Should_Succeed_When_Valid()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        var date = DateTime.UtcNow;

        // Act
        _todoList.RegisterProgression(DefaultItemId, date, 50m);

        // Assert
        var item = _todoList.Items.First(i => i.ItemId == DefaultItemId);
        item.GetTotalProgress().Should().Be(50m);
        item.Progressions.Should().ContainSingle(p => p.Percent == 50m && p.ActionDate == date);
    }

    [Fact]
    public void RegisterProgression_Should_Throw_When_DateIsNotSequential()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        var date1 = DateTime.UtcNow;
        var date2 = date1.AddMinutes(-10); // Earlier than date1
        _todoList.RegisterProgression(DefaultItemId, date1, 10m);

        // Act
        Action act = () => _todoList.RegisterProgression(DefaultItemId, date2, 10m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*must be later than the last progression date*");
    }

    [Fact]
    public void RegisterProgression_Should_Throw_When_PercentIsZeroOrNegative()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");

        // Act
        Action act = () => _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 0m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Percent must be greater than 0*");
    }

    [Fact]
    public void RegisterProgression_Should_Throw_When_TotalPercentExceeds100()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 80m);

        // Act
        Action act = () => _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow.AddMinutes(1), 30m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*exceed 100%*");
    }

    [Fact]
    public void IsCompleted_Should_BeTrue_When_ProgressIs100()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        
        // Act
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 100m);

        // Assert
        var item = _todoList.Items.First(i => i.ItemId == DefaultItemId);
        item.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void UpdateItem_Should_Succeed_When_ProgressIsBelow50()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 40m);

        // Act
        _todoList.UpdateItem(DefaultItemId, "New Description");

        // Assert
        var item = _todoList.Items.First(i => i.ItemId == DefaultItemId);
        item.Description.Should().Be("New Description");
    }

    [Fact]
    public void UpdateItem_Should_Throw_When_ProgressIsAbove50()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 60m);

        // Act
        Action act = () => _todoList.UpdateItem(DefaultItemId, "New Description");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*exceeds 50%*");
    }

    [Fact]
    public void RemoveItem_Should_Succeed_When_ProgressIsBelow50()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 40m);

        // Act
        _todoList.RemoveItem(DefaultItemId);

        // Assert
        _todoList.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_Should_Throw_When_ProgressIsAbove50()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        _todoList.RegisterProgression(DefaultItemId, DateTime.UtcNow, 60m);

        // Act
        Action act = () => _todoList.RemoveItem(DefaultItemId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*exceeds 50%*");
    }

    [Fact]
    public void PrintItems_Should_Format_Output_Correctly()
    {
        // Arrange
        _todoList.AddItem(DefaultItemId, "Title", "Desc", "Work");
        var date = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _todoList.RegisterProgression(DefaultItemId, date, 50m);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        _todoList.PrintItems();

        // Assert
        var result = sw.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Line 1: Item details
        lines[0].Should().Be("1) Title - Desc (Work) Completed:False");

        // Line 2: Progression (format might vary slightly depending on culture, so we check parts)
        // Expected: "01/01/2023 12:00:00 PM - 50%     |OOOOOOOOOOOOOOOOOOOOOOOOO                         |"
        // Note: The code uses InvariantCulture for date, so it should be M/d/yyyy
        lines[1].Should().Contain("1/1/2023 12:00:00 PM"); // M/d/yyyy
        lines[1].Should().Contain("50%");
        lines[1].Should().Contain("|OOOOOOOOOOOOOOOOOOOOOOOOO                         |"); // 25 'O's (50% of 50)
    }

    [Fact]
    public void PrintItems_Should_Match_Complex_Scenario()
    {
        // User Verification Scenario
        // Arrange
        _todoList.AddItem(DefaultItemId, "Complete Project Report", "Finish the final report for the project", "Work");
        
        // Progressions:
        // 2025-03-18, 30%
        // 2025-03-19, 50%
        // 2025-03-20, 20%
        var date1 = new DateTime(2025, 3, 18, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2025, 3, 19, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2025, 3, 20, 0, 0, 0, DateTimeKind.Utc);

        _todoList.RegisterProgression(DefaultItemId, date1, 30m);
        _todoList.RegisterProgression(DefaultItemId, date2, 50m);
        _todoList.RegisterProgression(DefaultItemId, date3, 20m);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        _todoList.PrintItems();

        // Assert
        var result = sw.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Header Line
        lines[0].Should().Be("1) Complete Project Report - Finish the final report for the project (Work) Completed:True");

        // Line 1: 3/18/2025 12:00:00 AM - 30%     |OOOOOOOOOOOOOOO                                   |
        // 30% of 50 = 15 chars
        lines[1].Should().Contain("3/18/2025 12:00:00 AM");
        lines[1].Should().Contain("30%");
        lines[1].Should().Contain("|OOOOOOOOOOOOOOO                                   |"); 

        // Line 2: 3/19/2025 12:00:00 AM - 80%     |OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO          |
        // 80% of 50 = 40 chars
        lines[2].Should().Contain("3/19/2025 12:00:00 AM");
        lines[2].Should().Contain("80%");
        lines[2].Should().Contain("|OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO          |");

        // Line 3: 3/20/2025 12:00:00 AM - 100%    |OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO|
        // 100% of 50 = 50 chars
        lines[3].Should().Contain("3/20/2025 12:00:00 AM");
        lines[3].Should().Contain("100%");
        lines[3].Should().Contain("|OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO|");
    }
}