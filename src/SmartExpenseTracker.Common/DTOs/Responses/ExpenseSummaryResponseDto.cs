namespace SmartExpenseTracker.Common.DTOs.Responses;

public class ExpenseSummaryResponseDto
{
    public decimal OverallTotal { get; set; }
    public int TotalCount { get; set; }
    public List<CategoryExpenseSummaryDto> CategoryBreakdown { get; set; } = new();
}
