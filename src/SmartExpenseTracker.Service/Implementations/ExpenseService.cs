using System.Globalization;
using SmartExpenseTracker.Common.Constants;
using SmartExpenseTracker.Common.DTOs.Requests;
using SmartExpenseTracker.Common.DTOs.Responses;
using SmartExpenseTracker.Common.Exceptions;
using SmartExpenseTracker.Common.Models;
using SmartExpenseTracker.Service.Abstractions;
using SmartExpenseTracker.Store.Abstractions;

namespace SmartExpenseTracker.Service.Implementations;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseStore _store;

    public ExpenseService(IExpenseStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<ExpenseResponseDto> AddExpenseAsync(CreateExpenseRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new Expense
        {
            Id = Guid.NewGuid(),
            Title = request.Title?.Trim(),
            Amount = request.Amount,
            Category = request.Category?.Trim(),
            Date = request.Date ?? DateTime.UtcNow
        };

        var savedEntity = await _store.AddAsync(entity, cancellationToken);
        return MapToDto(savedEntity);
    }

    public async Task<List<ExpenseResponseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default)
    {
        var expenses = await _store.GetAllAsync(cancellationToken);
        return expenses.Select(MapToDto).ToList();
    }

    public async Task<PagedResponseDto<ExpenseResponseDto>> GetPagedExpensesAsync(PagedRequestDto pagedRequest, string? category = null, CancellationToken cancellationToken = default)
    {
        pagedRequest ??= new PagedRequestDto();

        if (pagedRequest.PageNumber < 1)
        {
            throw new ValidationException(nameof(pagedRequest.PageNumber), "Page number must be at least 1.");
        }

        if (pagedRequest.PageSize < 1 || pagedRequest.PageSize > 100)
        {
            throw new ValidationException(nameof(pagedRequest.PageSize), "Page size must be between 1 and 100.");
        }

        var expenses = await _store.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(category))
        {
            expenses = expenses
                .Where(e => !string.IsNullOrEmpty(e.Category) && e.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalItems = expenses.Count;
        var pagedItems = expenses
            .OrderByDescending(e => e.Date)
            .Skip((pagedRequest.PageNumber - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResponseDto<ExpenseResponseDto>(pagedItems, totalItems, pagedRequest.PageNumber, pagedRequest.PageSize);
    }

    public async Task<PagedResponseDto<ExpenseResponseDto>> SearchExpensesAsync(string query, PagedRequestDto pagedRequest, CancellationToken cancellationToken = default)
    {
        pagedRequest ??= new PagedRequestDto();

        if (pagedRequest.PageNumber < 1)
        {
            throw new ValidationException(nameof(pagedRequest.PageNumber), "Page number must be at least 1.");
        }

        if (pagedRequest.PageSize < 1 || pagedRequest.PageSize > 100)
        {
            throw new ValidationException(nameof(pagedRequest.PageSize), "Page size must be between 1 and 100.");
        }

        var expenses = await _store.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.Trim();
            expenses = expenses
                .Where(e => (!string.IsNullOrEmpty(e.Title) && e.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Category) && e.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var totalItems = expenses.Count;
        var pagedItems = expenses
            .OrderByDescending(e => e.Date)
            .Skip((pagedRequest.PageNumber - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResponseDto<ExpenseResponseDto>(pagedItems, totalItems, pagedRequest.PageNumber, pagedRequest.PageSize);
    }

    public async Task<List<ExpenseResponseDto>> GetExpensesByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ValidationException(nameof(category), ValidationMessages.CategoryRequired);
        }

        var expenses = await _store.GetAllAsync(cancellationToken);
        return expenses
            .Where(e => !string.IsNullOrEmpty(e.Category) && e.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ExpenseSummaryResponseDto> GetExpenseSummaryAsync(CancellationToken cancellationToken = default)
    {
        var expenses = await _store.GetAllAsync(cancellationToken);

        var overallTotal = expenses.Sum(e => e.Amount);
        var totalCount = expenses.Count;

        var categoryBreakdown = expenses
            .Where(e => !string.IsNullOrWhiteSpace(e.Category))
            .GroupBy(e => e.Category!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new CategoryExpenseSummaryDto
            {
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .OrderBy(c => c.Category)
            .ToList();

        return new ExpenseSummaryResponseDto
        {
            OverallTotal = overallTotal,
            TotalCount = totalCount,
            CategoryBreakdown = categoryBreakdown
        };
    }

    public async Task<MonthlySummaryResponseDto> GetMonthlySummaryAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        var expenses = await _store.GetAllAsync(cancellationToken);

        if (year.HasValue && year.Value > 0)
        {
            expenses = expenses.Where(e => e.Date.Year == year.Value).ToList();
        }

        var overallTotal = expenses.Sum(e => e.Amount);
        var totalCount = expenses.Count;

        var monthlyBreakdown = expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlyExpenseSummaryDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                TotalAmount = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();

        return new MonthlySummaryResponseDto
        {
            OverallTotal = overallTotal,
            TotalCount = totalCount,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<bool> DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException(nameof(id), ValidationMessages.InvalidExpenseId);
        }

        var existing = await _store.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException(ValidationMessages.ExpenseNotFound);
        }

        return await _store.DeleteAsync(id, cancellationToken);
    }

    private static ExpenseResponseDto MapToDto(Expense entity)
    {
        return new ExpenseResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Amount = entity.Amount,
            Category = entity.Category,
            Date = entity.Date
        };
    }
}
