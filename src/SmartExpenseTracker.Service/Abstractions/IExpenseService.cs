using SmartExpenseTracker.Common.DTOs.Requests;
using SmartExpenseTracker.Common.DTOs.Responses;

namespace SmartExpenseTracker.Service.Abstractions;

public interface IExpenseService
{
    Task<ExpenseResponseDto> AddExpenseAsync(CreateExpenseRequestDto request, CancellationToken cancellationToken = default);
    Task<List<ExpenseResponseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default);
    Task<PagedResponseDto<ExpenseResponseDto>> GetPagedExpensesAsync(PagedRequestDto pagedRequest, string? category = null, CancellationToken cancellationToken = default);
    Task<PagedResponseDto<ExpenseResponseDto>> SearchExpensesAsync(string query, PagedRequestDto pagedRequest, CancellationToken cancellationToken = default);
    Task<List<ExpenseResponseDto>> GetExpensesByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<ExpenseSummaryResponseDto> GetExpenseSummaryAsync(CancellationToken cancellationToken = default);
    Task<MonthlySummaryResponseDto> GetMonthlySummaryAsync(int? year = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default);
}
