using Microsoft.AspNetCore.Mvc;
using SmartExpenseTracker.Common.DTOs.Requests;
using SmartExpenseTracker.Common.DTOs.Responses;
using SmartExpenseTracker.Service.Abstractions;

namespace SmartExpenseTracker.Controllers;

/// <summary>
/// REST API Endpoints for managing expense records, filtering, searching, and summary calculations (v1).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
    }

    /// <summary>
    /// Creates a new expense entry.
    /// </summary>
    /// <param name="request">Expense creation payload containing Title, Amount, Category, and optional Date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created expense details with assigned Guid.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/expenses
    ///     {
    ///        "title": "Ergonomic Desk Chair",
    ///        "amount": 299.99,
    ///        "category": "Office Supplies",
    ///        "date": "2026-07-31T10:00:00.000Z"
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExpenseResponseDto>> AddExpense([FromBody] CreateExpenseRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _expenseService.AddExpenseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAllExpenses), new { id = result.Id }, result);
    }

    /// <summary>
    /// View paginated expenses with optional category query string filter.
    /// </summary>
    /// <param name="pagedRequest">Pagination parameters (pageNumber, pageSize).</param>
    /// <param name="category">Optional category filter (e.g. Food, Travel, Office Supplies).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated response envelope containing items list and pagination metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<ExpenseResponseDto>>> GetAllExpenses(
        [FromQuery] PagedRequestDto pagedRequest,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetPagedExpensesAsync(pagedRequest, category, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Search expenses by title or category keywords (case-insensitive).
    /// </summary>
    /// <param name="query">Search term to look for within title or category.</param>
    /// <param name="pagedRequest">Pagination parameters (pageNumber, pageSize).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of expenses matching search query.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponseDto<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<ExpenseResponseDto>>> SearchExpenses(
        [FromQuery] string? query,
        [FromQuery] PagedRequestDto pagedRequest,
        CancellationToken cancellationToken)
    {
        var result = await _expenseService.SearchExpensesAsync(query ?? string.Empty, pagedRequest, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Filter paginated expenses by category route parameter.
    /// </summary>
    /// <param name="category">Category name to filter by (case-insensitive).</param>
    /// <param name="pagedRequest">Pagination parameters (pageNumber, pageSize).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of expenses matching specified category.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PagedResponseDto<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<ExpenseResponseDto>>> GetByCategory(
        string category,
        [FromQuery] PagedRequestDto pagedRequest,
        CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetPagedExpensesAsync(pagedRequest, category, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculates total expenses overall and categorized breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary response containing overall total, expense count, and category breakdown.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ExpenseSummaryResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseSummaryResponseDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _expenseService.GetExpenseSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Calculates monthly expense summaries grouped by year and month.
    /// </summary>
    /// <param name="year">Optional year filter (e.g. 2026).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Monthly summary response containing monthly breakdown list.</returns>
    [HttpGet("summary/monthly")]
    [ProducesResponseType(typeof(MonthlySummaryResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonthlySummaryResponseDto>> GetMonthlySummary(
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var summary = await _expenseService.GetMonthlySummaryAsync(year, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Deletes an expense entry by its unique Guid identifier.
    /// </summary>
    /// <param name="id">Unique Guid of the expense to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 No Content on successful deletion.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken cancellationToken)
    {
        await _expenseService.DeleteExpenseAsync(id, cancellationToken);
        return NoContent();
    }
}
