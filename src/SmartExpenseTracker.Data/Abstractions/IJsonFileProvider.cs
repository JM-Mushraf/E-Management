using SmartExpenseTracker.Common.Models;

namespace SmartExpenseTracker.Data.Abstractions;

public interface IJsonFileProvider
{
    Task<List<Expense>> ReadExpensesAsync(string fileName, CancellationToken cancellationToken = default);
    Task WriteExpensesAsync(string fileName, List<Expense> expenses, CancellationToken cancellationToken = default);
    string GetFilePath(string fileName);
}
