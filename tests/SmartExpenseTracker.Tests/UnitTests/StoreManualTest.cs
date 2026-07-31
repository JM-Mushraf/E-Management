using SmartExpenseTracker.Common.Models;
using SmartExpenseTracker.Data.Implementations;
using SmartExpenseTracker.Store.Implementations;
using Xunit;

namespace SmartExpenseTracker.Tests.UnitTests;

public class StoreManualTest
{
    [Fact]
    public async Task ExpenseStore_ReadWriteDelete_ShouldPersistCorrectly()
    {
        // Arrange
        var fileProvider = new JsonFileProvider();
        var store = new ExpenseStore(fileProvider);

        var testExpense = new Expense
        {
            Title = "Manual Store Test Expense",
            Amount = 150.75m,
            Category = "Testing",
            Date = DateTime.UtcNow
        };

        // Act 1: Add Expense
        var addedExpense = await store.AddAsync(testExpense);
        Assert.NotNull(addedExpense);
        Assert.NotEqual(Guid.Empty, addedExpense.Id);

        // Act 2: GetAll & Verify presence
        var allExpenses = await store.GetAllAsync();
        Assert.Contains(allExpenses, e => e.Id == addedExpense.Id);

        // Act 3: GetById
        var fetchedExpense = await store.GetByIdAsync(addedExpense.Id);
        Assert.NotNull(fetchedExpense);
        Assert.Equal("Manual Store Test Expense", fetchedExpense.Title);
        Assert.Equal(150.75m, fetchedExpense.Amount);

        // Act 4: Delete Expense
        var isDeleted = await store.DeleteAsync(addedExpense.Id);
        Assert.True(isDeleted);

        // Act 5: Verify deletion
        var afterDeleteAll = await store.GetAllAsync();
        Assert.DoesNotContain(afterDeleteAll, e => e.Id == addedExpense.Id);
    }
}
