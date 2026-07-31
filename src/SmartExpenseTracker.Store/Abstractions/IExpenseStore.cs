using SmartExpenseTracker.Common.Models;

namespace SmartExpenseTracker.Store.Abstractions;

public interface IExpenseStore
{
    Task<List<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAllAsync(List<Expense> expenses, CancellationToken cancellationToken = default);
}
