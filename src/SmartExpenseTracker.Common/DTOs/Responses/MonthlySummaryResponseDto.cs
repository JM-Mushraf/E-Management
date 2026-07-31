namespace SmartExpenseTracker.Common.DTOs.Responses;

public class MonthlySummaryResponseDto
{
    public decimal OverallTotal { get; set; }
    public int TotalCount { get; set; }
    public List<MonthlyExpenseSummaryDto> MonthlyBreakdown { get; set; } = new();
}
