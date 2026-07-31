using SmartExpenseTracker.Common.DTOs.Requests;
using SmartExpenseTracker.Common.Exceptions;
using SmartExpenseTracker.Common.Models;
using SmartExpenseTracker.Service.Implementations;
using SmartExpenseTracker.Store.Abstractions;
using Xunit;

namespace SmartExpenseTracker.Tests.UnitTests;

public class ExpenseServiceTests
{
    private class InMemoryExpenseStore : IExpenseStore
    {
        private readonly List<Expense> _expenses = new();

        public Task<List<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_expenses.ToList());
        }

        public Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var match = _expenses.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(match);
        }

        public Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            _expenses.Add(expense);
            return Task.FromResult(expense);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existing = _expenses.FirstOrDefault(e => e.Id == id);
            if (existing == null) return Task.FromResult(false);
            _expenses.Remove(existing);
            return Task.FromResult(true);
        }

        public Task SaveAllAsync(List<Expense> expenses, CancellationToken cancellationToken = default)
        {
            _expenses.Clear();
            _expenses.AddRange(expenses);
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryExpenseStore _store;
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _store = new InMemoryExpenseStore();
        _service = new ExpenseService(_store);
    }

    [Fact]
    public async Task AddExpense_WithValidData_ShouldCreateExpense()
    {
        // Arrange
        var request = new CreateExpenseRequestDto
        {
            Title = "Laptop Charger",
            Amount = 45.99m,
            Category = "Electronics"
        };

        // Act
        var result = await _service.AddExpenseAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Laptop Charger", result.Title);
        Assert.Equal(45.99m, result.Amount);
        Assert.Equal("Electronics", result.Category);
    }

    [Fact]
    public async Task AddExpense_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddExpenseAsync(null!));
    }

    [Fact]
    public async Task DeleteExpense_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var added = await _service.AddExpenseAsync(new CreateExpenseRequestDto
        {
            Title = "Monitor",
            Amount = 199.99m,
            Category = "Electronics"
        });

        // Act
        var isDeleted = await _service.DeleteExpenseAsync(added.Id);

        // Assert
        Assert.True(isDeleted);
        var paged = await _service.GetPagedExpensesAsync(new PagedRequestDto());
        Assert.Empty(paged.Items);
    }

    [Fact]
    public async Task DeleteExpense_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteExpenseAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task FilterCategory_ShouldReturnMatchingCategoryExpensesCaseInsensitively()
    {
        // Arrange
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Pizza", Amount = 20m, Category = "Food" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Burger", Amount = 12m, Category = "food" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Taxi", Amount = 30m, Category = "Travel" });

        // Act
        var foodExpenses = await _service.GetPagedExpensesAsync(new PagedRequestDto(), "FOOD");

        // Assert
        Assert.Equal(2, foodExpenses.TotalItems);
        Assert.All(foodExpenses.Items, e => Assert.Equal("food", e.Category, ignoreCase: true));
    }

    [Fact]
    public async Task CalculateTotal_ShouldReturnOverallAndCategoryBreakdown()
    {
        // Arrange
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Lunch", Amount = 25m, Category = "Food" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Dinner", Amount = 35m, Category = "Food" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Subway", Amount = 10m, Category = "Travel" });

        // Act
        var summary = await _service.GetExpenseSummaryAsync();

        // Assert
        Assert.Equal(70m, summary.OverallTotal);
        Assert.Equal(3, summary.TotalCount);
        Assert.Equal(2, summary.CategoryBreakdown.Count);

        var foodSummary = summary.CategoryBreakdown.First(c => c.Category != null && c.Category.Equals("Food", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(60m, foodSummary.TotalAmount);
        Assert.Equal(2, foodSummary.Count);
    }

    [Fact]
    public async Task GetPagedExpenses_ShouldReturnPaginatedResultsAndMetadata()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await _service.AddExpenseAsync(new CreateExpenseRequestDto
            {
                Title = $"Expense {i}",
                Amount = 10m * i,
                Category = i % 2 == 0 ? "Food" : "Travel"
            });
        }

        // Act (Page 1, Size 5)
        var pagedResult = await _service.GetPagedExpensesAsync(new PagedRequestDto { PageNumber = 1, PageSize = 5 });

        // Assert
        Assert.NotNull(pagedResult);
        Assert.Equal(15, pagedResult.TotalItems);
        Assert.Equal(3, pagedResult.TotalPages);
        Assert.Equal(1, pagedResult.PageNumber);
        Assert.Equal(5, pagedResult.PageSize);
        Assert.Equal(5, pagedResult.Items.Count);
        Assert.True(pagedResult.HasNextPage);
        Assert.False(pagedResult.HasPreviousPage);
    }

    [Fact]
    public async Task SearchExpenses_ShouldReturnMatchingTitleOrCategoryResults()
    {
        // Arrange
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Wireless Mouse", Amount = 25m, Category = "Hardware" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Keyboard", Amount = 45m, Category = "Hardware" });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto { Title = "Coffee Cup", Amount = 5m, Category = "Office Supplies" });

        // Act
        var searchResult = await _service.SearchExpensesAsync("mouse", new PagedRequestDto());

        // Assert
        Assert.Single(searchResult.Items);
        Assert.Equal("Wireless Mouse", searchResult.Items[0].Title);
    }

    [Fact]
    public async Task GetMonthlySummary_ShouldGroupExpensesByYearAndMonth()
    {
        // Arrange
        await _service.AddExpenseAsync(new CreateExpenseRequestDto
        {
            Title = "January Lunch",
            Amount = 50m,
            Category = "Food",
            Date = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto
        {
            Title = "January Dinner",
            Amount = 75m,
            Category = "Food",
            Date = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)
        });
        await _service.AddExpenseAsync(new CreateExpenseRequestDto
        {
            Title = "February Groceries",
            Amount = 120m,
            Category = "Food",
            Date = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)
        });

        // Act
        var monthlySummary = await _service.GetMonthlySummaryAsync(2026);

        // Assert
        Assert.Equal(245m, monthlySummary.OverallTotal);
        Assert.Equal(3, monthlySummary.TotalCount);
        Assert.Equal(2, monthlySummary.MonthlyBreakdown.Count);

        var janBreakdown = monthlySummary.MonthlyBreakdown.First(m => m.Month == 1);
        Assert.Equal(125m, janBreakdown.TotalAmount);
        Assert.Equal(2, janBreakdown.Count);
        Assert.Equal("January", janBreakdown.MonthName);
    }
}
