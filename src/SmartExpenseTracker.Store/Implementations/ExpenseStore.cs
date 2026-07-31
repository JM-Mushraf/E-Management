using SmartExpenseTracker.Common.Constants;
using SmartExpenseTracker.Common.Models;
using SmartExpenseTracker.Data.Abstractions;
using SmartExpenseTracker.Store.Abstractions;

namespace SmartExpenseTracker.Store.Implementations;

public class ExpenseStore : IExpenseStore
{
    private readonly IJsonFileProvider _fileProvider;
    private readonly string _fileName;

    public ExpenseStore(IJsonFileProvider fileProvider)
    {
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _fileName = JsonFileNames.ExpensesJson;
    }

    public async Task<List<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _fileProvider.ReadExpensesAsync(_fileName, cancellationToken);
    }

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expenses = await GetAllAsync(cancellationToken);
        return expenses.FirstOrDefault(e => e.Id == id);
    }

    public async Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        var expenses = await GetAllAsync(cancellationToken);
        expenses.Add(expense);
        await SaveAllAsync(expenses, cancellationToken);
        return expense;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expenses = await GetAllAsync(cancellationToken);
        var existingExpense = expenses.FirstOrDefault(e => e.Id == id);

        if (existingExpense == null)
        {
            return false;
        }

        expenses.Remove(existingExpense);
        await SaveAllAsync(expenses, cancellationToken);
        return true;
    }

    public async Task SaveAllAsync(List<Expense> expenses, CancellationToken cancellationToken = default)
    {
        await _fileProvider.WriteExpensesAsync(_fileName, expenses, cancellationToken);
    }
}
